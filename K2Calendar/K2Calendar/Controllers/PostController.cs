using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using K2Calendar.Models;
using System.Web.Script.Serialization;
using System.Text;
using System.Web.Security;
using System.Diagnostics;

namespace K2Calendar.Controllers
{ 
    public class PostController : Controller
    {
        private AppDbContext dbContext = new AppDbContext();

        //TODO: index, show recent posts in a summary type format
        // GET: /Post/
        public ViewResult Index()
        {
            var posts = dbContext.Posts.Include(p => p.Rank);
            return View(posts.ToList());
        }

        // GET: /Post/Details/5
        [Authorize]
        public ViewResult Details(int id)
        {
           PostModel postmodel = dbContext.Posts.Include("Rank").Include("PostedBy").Single(p => p.Id == id);
           ViewBag.ExistingTags = FormatExistingTags(postmodel);
           return View(postmodel);
        }

        // GET: /Post/Create
        [Authorize(Roles="Administrator,SuperAdmin")]
        public ActionResult Create()
        {
            AccountController.GenerateRanksList(dbContext, ViewBag);
            return View();
        } 

        // POST: /Post/Create
        [HttpPost]
        [Authorize(Roles = "Administrator,SuperAdmin")]
        public ActionResult Create(PostModel postmodel)
        {
            postmodel.Tags = new List<TagModel>();
            ProcessTags(postmodel);

            if (ModelState.IsValid)
            {
                postmodel.PostDate = DateTime.Now.ToUniversalTime();
                postmodel.EventDate = postmodel.EventDate.ToUniversalTime();
                postmodel.PosterId = AccountController.GetUserInfoFromMembershipUser(Membership.GetUser(), dbContext).Id;
                
                dbContext.Posts.Add(postmodel);
                dbContext.SaveChanges();

                return RedirectToAction("Details", new { id = postmodel.Id });  
            }

            AccountController.GenerateRanksList(dbContext, ViewBag);
            ViewBag.ExistingTags = FormatExistingTags(postmodel);
            return View(postmodel);
        }

        // GET: /Post/Edit/5
        [Authorize(Roles = "Administrator,SuperAdmin")]
        public ActionResult Edit(int id)
        {
            PostModel postmodel = dbContext.Posts.Find(id);
            AccountController.GenerateRanksList(dbContext, ViewBag);
            ViewBag.ExistingTags = FormatExistingTags(postmodel);
            return View(postmodel);
        }

        // POST: /Post/Edit/5
        //TODO: enable setting IsActive flag, add admin panel dropdown button, shown when user is admin, allows deleting of post
        [HttpPost]
        [Authorize(Roles = "Administrator,SuperAdmin")]
        public ActionResult Edit(PostModel updatedModel)
        {
            if (ModelState.IsValid)
            {
                PostModel originalModel = dbContext.Posts.Find(updatedModel.Id);
                updatedModel.IsActive = originalModel.IsActive;
                updatedModel.PostDate = originalModel.PostDate;
                updatedModel.PosterId = originalModel.PosterId;
                updatedModel.Tags = new List<TagModel>();
                ProcessTags(updatedModel);
                
                originalModel.Tags.Clear();
                foreach (TagModel tag in updatedModel.Tags)
                {
                    originalModel.Tags.Add(tag);
                }
                //even though TagsInput is not mapped it is required so must have value to allow save
                originalModel.TagsInput = updatedModel.TagsInput; 
              
                try
                {
                    dbContext.Entry(originalModel).CurrentValues.SetValues(updatedModel);
                    dbContext.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
                return RedirectToAction("Details", new { id = updatedModel.Id }); 
            }
            AccountController.GenerateRanksList(dbContext, ViewBag);
            ViewBag.ExistingTags = FormatExistingTags(updatedModel);
            return View(updatedModel);
        }

        
        //TODO: delete
        // GET: /Post/Delete/5
        public ActionResult Delete(int id)
        {
            PostModel postmodel = dbContext.Posts.Find(id);
            return View(postmodel);
        }

        // POST: /Post/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            PostModel postmodel = dbContext.Posts.Find(id);
            dbContext.Posts.Remove(postmodel);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Processes TagsInput property of PostModel
        /// </summary>
        private void ProcessTags(PostModel postmodel)
        {
            //existing tag comes in as ID whereas new tag comes in as new tag name
            string[] tagInputs = postmodel.TagsInput.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string tagOrId in tagInputs)
            {
                int tagId;
                TagModel tagForPost;

                //id of existing tag
                if (int.TryParse(tagOrId, out tagId))
                {
                    tagForPost = dbContext.Tags.Find(tagId);
                    //tagForPost will be null if user passes in int as new tag name which we don't support
                }
                //new tag
                else
                {
                    tagForPost = new TagModel { Name = tagOrId, IsActive = true };
                    dbContext.Tags.Add(tagForPost);
                    dbContext.SaveChanges();
                }

                if (tagForPost != null)
                {
                    postmodel.Tags.Add(tagForPost);
                }
            }
        }
      
        /// <summary>
        /// Called ajaxy from TagsInput on Post forms to retrieve tags from db
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public string SearchTags(string q)
        {
            var query = from cat in dbContext.Tags
                        where cat.Name.Contains(q)
                        select cat;

            List<TagHolderForJson> tags = new List<TagHolderForJson>();
            foreach (TagModel model in query)
	        {
		        tags.Add(new TagHolderForJson { id=model.Id.ToString(), name = model.Name});
	        }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(tags);
        }

        /// <summary>
        /// Creates the pre-populated tags property for the JQuery TokenInput control used to render Tags for a Post
        /// </summary>
        public string FormatExistingTags(PostModel model)
        {
            string existingTags = "";
            if (model.Tags != null || model.Tags.Count > 0)
            {
                System.Text.StringBuilder prePopulate = new System.Text.StringBuilder("prePopulate: [ \n");
                foreach (var tag in model.Tags)
                {
                    string tagString = string.Format(@"{{id:{0}, name:""{1}""}},", tag.Id, tag.Name);
                    prePopulate.Append(tagString);
                }
                prePopulate.Remove(prePopulate.Length - 1, 1); //remove trailing comma
                prePopulate.Append("]");
                existingTags = prePopulate.ToString();
            }
            return existingTags;
        }

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}

//Used to hold a TagModel and lowercase the property names
//so when serialized the data fits in perfectly for jquery.tokeninput
public class TagHolderForJson
{
    public string id { get; set; }
    public string name { get; set; }
}