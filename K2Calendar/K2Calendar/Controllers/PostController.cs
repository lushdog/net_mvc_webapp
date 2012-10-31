using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using K2Calendar.Models;
using Microsoft.Security.Application;

namespace K2Calendar.Controllers
{ 
    public class PostController : Controller
    {
        private AppDbContext dbContext = new AppDbContext();
        private const int PAGE_SIZE = 5;
        private const int DESCRIPTION_SUMMARY_LENGTH = 400;
        
        // GET: /Post/
        //TODO: list posts like we do users
        [Authorize]
        public ViewResult Index(int pageNumber = 1)
        {
            ViewBag.NumPages = Math.Ceiling((double)dbContext.Posts.Count() / PAGE_SIZE);
            ViewBag.PageNum = pageNumber;
            var posts = dbContext.Posts.Include(p => p.Rank).Include(p => p.PostedBy).Where(p => p.IsActive == true).OrderByDescending(p => p.Id).Skip((pageNumber - 1) * PAGE_SIZE).Take(PAGE_SIZE);
            foreach (PostModel post in posts)
            {
                if (post.Description.Length > DESCRIPTION_SUMMARY_LENGTH)
                {
                    var d = post.Description.Remove(DESCRIPTION_SUMMARY_LENGTH - 3);
                    post.Description  = d + "...";
                }
            }
            return View(posts.ToList());
        }

        //TODO: implement SEARCH post and get controller actions

        // GET: /Post/Details/5
        [Authorize]
        public ActionResult Details(int id)
        {
           PostModel postmodel = dbContext.Posts.Include(p => p.Rank).Include(p => p.PostedBy).Single(p => p.Id == id);
           if (postmodel.IsActive == false)
           {
               return Content("This post is no longer available");
           }
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
                postmodel.IsActive = true;
                postmodel.Description = Encoder.HtmlEncode(postmodel.Description);
                dbContext.Posts.Add(postmodel);
                dbContext.SaveChanges();

                return RedirectToAction("Details", new { id = postmodel.Id });  
            }

            AccountController.GenerateRanksList(dbContext, ViewBag);
            return View(postmodel);
        }

        // GET: /Post/Edit/5
        [Authorize(Roles = "Administrator,SuperAdmin")]
        public ActionResult Edit(int id)
        {
            PostModel postmodel = dbContext.Posts.Find(id);
            AccountController.GenerateRanksList(dbContext, ViewBag);
            return View(postmodel);
        }

        // POST: /Post/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrator,SuperAdmin")]
        public ActionResult Edit(PostModel updatedModel)
        {
            if (ModelState.IsValid)
            {
                PostModel originalModel = dbContext.Posts.Find(updatedModel.Id);
                updatedModel.PostDate = originalModel.PostDate;
                updatedModel.PosterId = originalModel.PosterId;
                updatedModel.Description = Encoder.HtmlEncode(updatedModel.Description);
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
            return View(updatedModel);
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
                    //dbContext.SaveChanges();
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
            var query = from tag in dbContext.Tags
                        where tag.Name.Contains(q)
                        select tag;

            List<TagHolderForJson> tags = new List<TagHolderForJson>();
            foreach (TagModel model in query)
	        {
		        tags.Add(new TagHolderForJson { id=model.Id.ToString(), name = model.Name});
	        }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(tags);
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