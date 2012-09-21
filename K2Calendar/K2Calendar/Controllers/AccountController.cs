using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using K2Calendar.Models;

namespace K2Calendar.Controllers
{
    public class AccountController : Controller
    {
        AppDbContext dbContext = new AppDbContext();

        // GET: /Account/LogOn
        public ActionResult LogOn()
        {
            return View();
        }

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

        // GET: /Account/LogOff
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [Authorize(Roles="Administrator,SuperAdmin")]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [Authorize]
        public ActionResult Register(CreateUserModel model)
        {
            if (ModelState.IsValid)
            {
                UserInfoModel userInfoModel = model.UserInfoModel;
                
                // Attempt to register the user in membership provider
                MembershipCreateStatus createStatus;
                MembershipUser newUser = Membership.CreateUser(model.UserName, model.Password, model.Email, null, null, true, null, out createStatus);
              
                if (createStatus == MembershipCreateStatus.Success)
                {
                    try
                    {
                        userInfoModel.MembershipId = new Guid(newUser.ProviderUserKey.ToString());
                        userInfoModel.BirthDate = userInfoModel.BirthDate.ToUniversalTime();
                        userInfoModel.EnrollmentDate = userInfoModel.EnrollmentDate.ToUniversalTime();
                        userInfoModel.SignUpDate = DateTime.Now.ToUniversalTime();
                        userInfoModel.IsActive = true;
                        userInfoModel.RankId = dbContext.Ranks.Where(r => r.IsActive == true).Min(r => r.Level);
                        dbContext.Users.Add(userInfoModel);
                        dbContext.SaveChanges();
                       Roles.AddUserToRole(model.UserName, "User");
                    }
                    catch (Exception ex)
                    {
                        Membership.DeleteUser(model.UserName);
                        throw new InvalidOperationException("Could not create UserInfoModel.", ex);
                    }

                    //TODO: remove comment below when user's are allowed to register and not just admins
                    //FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                    
                    TempData["isSuccessRegister"] = true;
                    return RedirectToAction("Admin", new { id = GetUserInfoFromMembershipUser(newUser, dbContext).Id });
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/Edit
        [Authorize]
        public ActionResult Edit(int id)
        {
            if (GetUserInfoFromMembershipUser(Membership.GetUser(), dbContext).Id != id)
                throw new InvalidOperationException("User does not have permission to edit a different user's account.");
              
            UserInfoModel userToEdit = dbContext.Users.Include("Rank").Single(u => u.Id == id);

            if (userToEdit == null)
                throw new InvalidOperationException("Could not find UserInfo for provided Id.");

            MembershipUser membershipUser = Membership.GetUser(userToEdit.MembershipId);

            if (membershipUser == null)
                throw new InvalidOperationException("Could not find MembershipUser for provided MembershipId.");

            EditUserInfoModel editUserInfoModel = new EditUserInfoModel { Email = membershipUser.Email, UserName = membershipUser.UserName };
            editUserInfoModel.UserInfoModel = userToEdit;

            return View(editUserInfoModel);
        }

        // POST: /Account/Edit
        [Authorize]
        [HttpPost]
        public ActionResult Edit(EditUserInfoModel model)
        {
            UserInfoModel currentUserInfo = GetUserInfoFromMembershipUser(Membership.GetUser(), dbContext);
            if (currentUserInfo.Id != model.UserInfoModel.Id)
                throw new InvalidOperationException("User does not have permission to edit a different user's account.");
          
            if (ModelState.IsValid)
            {
                try
                {
                    //following fields are not editable by user
                    model.UserInfoModel.RankId = currentUserInfo.RankId;
                    model.UserInfoModel.SignUpDate = currentUserInfo.SignUpDate;
                    model.UserInfoModel.MembershipId = currentUserInfo.MembershipId;
                    model.UserInfoModel.EnrollmentDate = currentUserInfo.EnrollmentDate;

                    model.UserInfoModel.BirthDate = model.UserInfoModel.BirthDate.ToUniversalTime();
                    
                    dbContext.Entry(currentUserInfo).CurrentValues.SetValues(model.UserInfoModel);
                    dbContext.SaveChanges();
                    TempData["isSuccessEdit"] = true;
                    return RedirectToAction("Edit");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to update UserInfoModel", ex);
                }
            }
            return View(model);
        }

        // GET: /Account/Admin/
        [Authorize(Roles = "Administrator,SuperAdmin")]
        public ActionResult Admin(int id)
        {
            UserInfoModel userToAdmin = dbContext.Users.Find(id);
            GenerateRanksList(dbContext, ViewBag);
            GenerateRoleList();

            if (userToAdmin == null)
                throw new InvalidOperationException("Could not find UserInfo for provided Id.");

            MembershipUser membershipUser = Membership.GetUser(userToAdmin.MembershipId);

            if (membershipUser == null)
                throw new InvalidOperationException("Could not find MembershipUser for provided MembershipId.");

            AdminUserInfoModel model = new AdminUserInfoModel { Email = membershipUser.Email, UserName = membershipUser.UserName, Role = Roles.GetRolesForUser(membershipUser.UserName).Single() };
            model.UserInfoModel = userToAdmin;
            return View(model);
        }

        // POST: /Account/Admin/
        [Authorize(Roles = "Administrator,SuperAdmin")]
        [HttpPost]
        //TODO: make user inactive
        public ActionResult Admin(AdminUserInfoModel model)
        {
            UserInfoModel userToUpdate = dbContext.Users.Find(model.UserInfoModel.Id);

            if (ModelState.IsValid)
            {
                try
                {
                    model.UserInfoModel.MembershipId = userToUpdate.MembershipId;
                    MembershipUser membershipUserToUpdate = Membership.GetUser(userToUpdate.MembershipId);
                    Roles.RemoveUserFromRoles(membershipUserToUpdate.UserName, Roles.GetRolesForUser(membershipUserToUpdate.UserName));
                    Roles.AddUserToRole(membershipUserToUpdate.UserName, model.Role);
                    dbContext.Entry(userToUpdate).CurrentValues.SetValues(model.UserInfoModel);
                    dbContext.SaveChanges();
                    TempData["isSuccessAdmin"] = true;
                    return RedirectToAction("Admin");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to update UserInfoModel", ex);
                }
            }
            GenerateRanksList(dbContext, ViewBag);
            GenerateRoleList();
            return View(model);
        }
        
        // GET: /Account/ChangePassword
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

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
                
        // GET: /Account/ChangePasswordSuccess
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }
        

        //Account helper methods

        public static UserInfoModel GetUserInfoFromMembershipUser(MembershipUser user, AppDbContext dbContext)
        {
            Guid currentUserKey = new Guid(user.ProviderUserKey.ToString());
            return dbContext.Users.Include("Rank").Single(u => u.MembershipId == currentUserKey);
        }
    
        public static void GenerateRanksList(AppDbContext dbContext, dynamic viewBag, object selectedRankId = null)
        {
            //TODO: handle a user who has a rank that is not active
            //note that you can't set a SelectListItem to disabled from here (that I know of)
            var ranksQuery = from ranks in dbContext.Ranks
                             orderby ranks.Level ascending
                             select ranks;
            viewBag.RankList = new SelectList(ranksQuery, "Id", "Name", selectedRankId);
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
