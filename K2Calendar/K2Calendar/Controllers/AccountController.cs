using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using K2Calendar.Models;
using System.Data.Services.Client;
using System.Data.Objects;

namespace K2Calendar.Controllers
{
    public class AccountController : Controller
    {

        AppDbContext dbContext = new AppDbContext();

        //
        // GET: /Account/LogOn

        public ActionResult LogOn()
        {
            return View();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                       return RedirectToAction("Register", "Account");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [Authorize]
        public ActionResult Register(UserInfoAndRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                CreateMembershipModel registerModel = model.RegisterModel;
                UserInfoModel userInfoModel = model.UserInfoModel;
                
                // Attempt to register the user in membership provider
                MembershipCreateStatus createStatus;
                MembershipUser newUser = Membership.CreateUser(registerModel.UserName, registerModel.Password, registerModel.Email, null, null, true, null, out createStatus);
              
                if (createStatus == MembershipCreateStatus.Success)
                {
                    try
                    {
                        userInfoModel.MembershipId = new Guid(newUser.ProviderUserKey.ToString());
                        userInfoModel.SignUpDate = DateTime.Now.ToUniversalTime();
                        userInfoModel.IsActive = true;
                        userInfoModel.RankId = dbContext.Ranks.Min(r => r.Level);
                        dbContext.Users.Add(userInfoModel);
                        dbContext.SaveChanges();
                        Roles.AddUserToRole(registerModel.UserName, "User");
                    }
                    catch (Exception ex)
                    {
                        Membership.DeleteUser(registerModel.UserName);
                        throw new InvalidOperationException("Could not create UserInfoModel.", ex);
                    }

                    FormsAuthentication.SetAuthCookie(registerModel.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Edit
       
        public ActionResult Edit(int id)
        {
            if (GetCurrentUserInfo(Membership.GetUser()).Id != id)
                throw new InvalidOperationException("User does not have permission to edit a different user's account.");
              
            UserInfoModel userInfoModel = dbContext.Users.Find(id);

            if (userInfoModel == null)
                throw new InvalidOperationException("Could not find UserInfo for provided Id.");

            MembershipUser membershipUser = Membership.GetUser(userInfoModel.MembershipId);

            if (userInfoModel == null)
                throw new InvalidOperationException("Could not find MembershipUser for provided MembershipId.");

            UserEditInfoModel userEditInfoModel = new UserEditInfoModel { Email = membershipUser.Email, UserName = membershipUser.UserName };
            userEditInfoModel.UserInfoModel = userInfoModel;
            return View(userEditInfoModel);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(UserEditInfoModel model)
        {
            UserInfoModel currentUserInfo = GetCurrentUserInfo(Membership.GetUser());
            if (currentUserInfo.Id != model.UserInfoModel.Id)
                throw new InvalidOperationException("User does not have permission to edit a different user's account.");
          
            if (ModelState.IsValid)
            {
                try
                {
                    model.UserInfoModel.RankId = currentUserInfo.RankId;
                    dbContext.Entry(currentUserInfo).CurrentValues.SetValues(model.UserInfoModel);
                    dbContext.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to update UserInfoModel", ex);
                }
            }
            return View(model);
        }

        public UserInfoModel GetCurrentUserInfo(MembershipUser currentUser)
        {
            Guid currentUserKey = new Guid(currentUser.ProviderUserKey.ToString());
            return dbContext.Users.Single(u => u.MembershipId == currentUserKey);
        }
       
        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }



        private void GenerateRanksList(object selectedRankId = null)
        {
            var ranksQuery = from ranks in dbContext.Ranks
                             orderby ranks.Level ascending
                             select ranks;
            ViewBag.RankList = new SelectList(ranksQuery, "Id", "Name", selectedRankId);
        }

        private void GenerateRoleList(object selectedRoleName = null)
        {
            string[] roleNames = Roles.GetAllRoles();
            ViewBag.RoleList = new SelectList(roleNames, selectedRoleName);
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
