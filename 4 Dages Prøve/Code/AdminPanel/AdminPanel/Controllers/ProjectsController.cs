using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using AdminPanel.Filters;
using System.Web.Security;

namespace AdminPanel.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class ProjectsController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /Projects/

        public ActionResult Index()
        {
            var posts = db.Posts.Include(p => p.PostType).Include(p => p.Status).Include(p => p.UserProfile).Where(p => p.FK_Type == 2);
            return View(posts.ToList());
        }

        //
        // GET: /Projects/Details/5

        public ActionResult Details(int id = 0)
        {
            Posts posts = db.Posts.Find(id);
            if (posts == null)
            {
                return HttpNotFound();
            }
            return View(posts);
        }

        //
        // GET: /Projects/Create

        public ActionResult Create()
        {
            ViewBag.FK_Type = new SelectList(db.PostType, "Id", "Name");
            ViewBag.FK_Status = new SelectList(db.Status, "Id", "Name");

            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Media = db.Media.ToList();
            return View(new Posts());
        }

        //
        // POST: /Projects/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Posts posts)
        {
            posts.DateCreated = DateTime.Now;
            posts.FK_Type = 2;
            posts.FK_User = (int)Membership.GetUser().ProviderUserKey;
            if (posts.FK_Status == 0)
            {
                posts.FK_Status = db.Categories.FirstOrDefault().Id;
            }
            db.Posts.Add(posts);

            if(Request.Form["imagestoadd"] != null) {
                string mediasquery = Request.Form["imagestoadd"].Substring(0, Request.Form["imagestoadd"].Length - 1);
                string[] medias = mediasquery.Split(',');
                foreach (string itm in medias)
                {
                    int id = int.Parse(itm);
                    if (db.MediaInPosts.Where(p => p.Fk_Posts == posts.Id && p.Fk_Media == id).Count() == 0)
                    {
                        MediaInPosts AddMedia = new MediaInPosts();
                        AddMedia.Fk_Media = id;
                        AddMedia.Fk_Posts = posts.Id;
                        db.MediaInPosts.Add(AddMedia);
                    }

                }
            }

            if (Request.Form["Category"] != null)
            {
                string[] categories = Request.Form["Category"].Split(',');
                foreach (string itm in categories)
                {
                    int id = int.Parse(itm);
                    if (db.CategoriesInPosts.Where(p => p.FK_Posts == posts.Id && p.FK_Categories == id).Count() == 0)
                    {
                        CategoriesInPosts AddCategory = new CategoriesInPosts();
                        AddCategory.FK_Categories = id;
                        AddCategory.FK_Posts = posts.Id;
                        db.CategoriesInPosts.Add(AddCategory);
                    }
                }

            }
            //Delete All Tags found for the post
            foreach (TagsInPosts tagini in db.TagsInPosts.Where(p => p.FK_Posts == posts.Id).ToList())
            {
                db.TagsInPosts.Remove(tagini);

            }
            //is any tags have been givin
            if (posts.tags != null)
            {
                //Tags
                string TagTrim = posts.tags.ToString().Replace(" ", string.Empty).ToLower();
                string[] tags = TagTrim.Split(',');
                foreach (string tag in tags)
                {
                    if (db.Tags.Where(p => p.Name.Contains(tag)).Count() == 0)
                    {
                        DA.Tags addtag = new DA.Tags();
                        addtag.Name = tag.ToLower();
                        db.Tags.Add(addtag);
                        db.SaveChanges();

                        DA.TagsInPosts AddToItem = new DA.TagsInPosts();
                        AddToItem.FK_Posts = posts.Id;
                        AddToItem.FK_Tag = addtag.Id;
                        db.TagsInPosts.Add(AddToItem);
                        db.SaveChanges();
                    }
                    else
                    {
                        //Get Id of Tag
                        int Tagid = db.Tags.Where(p => p.Name.Contains(tag)).FirstOrDefault().Id;
                        if (db.TagsInPosts.Where(p => p.FK_Tag == Tagid && p.FK_Posts == posts.Id).Count() == 0)
                        {
                            DA.TagsInPosts AddToItem = new DA.TagsInPosts();
                            AddToItem.FK_Posts = posts.Id;
                            AddToItem.FK_Tag = db.Tags.Where(p => p.Name.Contains(tag)).FirstOrDefault().Id;
                            db.TagsInPosts.Add(AddToItem);
                            db.SaveChanges();
                        }

                    }
                }
            }

            db.SaveChanges();
            return RedirectToAction("Index");
            
        }

        //
        // GET: /Projects/Edit/5

        public ActionResult DelImage(int id = 0, int imageid = 0)
        {
            Media media = db.Media.Find(imageid);
            if (media != null)
            {
               MediaInPosts delpost =  db.MediaInPosts.Where(p => p.Fk_Posts == id && p.Fk_Media == imageid).FirstOrDefault();
               db.MediaInPosts.Remove(delpost);

               db.SaveChanges();
               return RedirectToAction("Edit", new { id = id });
            }

            return HttpNotFound();
        }
        public ActionResult Edit(int id = 0)
        {
            Posts posts = db.Posts.Find(id);
            if (posts == null)
            {
                return HttpNotFound();
            }
            //Medias
            var medias = (from p in db.Media
                          select new MediaInPost_Checkup(){
                              Name = p.Name,
                              Id = p.Id,
                              isChecked = p.MediaInPosts.Any(o => o.Fk_Posts == id),
                              path = p.Path
                          }).ToList();

            ViewBag.Media = medias;




            //Categories
            var d = (from p in db.Categories
                     select new CategoryInPost_Checkup()
                     {
                         Name = p.Name,
                         Id = p.Id,
                         isChecked = p.CategoriesInPosts.Any(o => o.FK_Posts == id)
                     }).ToList();

            ViewBag.Categories = d;


            //Tags
            string Gettags = "";
            foreach (TagsInPosts tagsin in db.TagsInPosts.Where(p => p.FK_Posts == id).ToList())
            {
                Tags tags = db.Tags.Where(p => p.Id == tagsin.FK_Tag).FirstOrDefault();
                Gettags += tags.Name + ",";
            }

            if (Gettags.Length != 0)
            {
                posts.tags = Gettags.Substring(0, Gettags.Length - 1);
            }

            ViewBag.FK_Type = new SelectList(db.PostType, "Id", "Name", posts.FK_Type);
            ViewBag.FK_Status = new SelectList(db.Status, "Id", "Name", posts.FK_Status);
            
            return View(posts);
        }

        //
        // POST: /Projects/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Posts posts)
        {
            if (ModelState.IsValid)
            {
                posts.FK_User = (int)Membership.GetUser().ProviderUserKey;
                db.Entry(posts).State = EntityState.Modified;
                db.SaveChanges();


                if (Request.Form["imagestoadd"] != null)
                {

                    string mediasquery = Request.Form["imagestoadd"].Substring(0, Request.Form["imagestoadd"].Length - 1);
                    string[] medias = mediasquery.Split(',');
                    foreach (string itm in medias)
                    {
                        int id = int.Parse(itm);
                        if (db.MediaInPosts.Where(p => p.Fk_Posts == posts.Id && p.Fk_Media == id).Count() == 0)
                        {
                            MediaInPosts AddMedia = new MediaInPosts();
                            AddMedia.Fk_Media = id;
                            AddMedia.Fk_Posts = posts.Id;
                            db.MediaInPosts.Add(AddMedia);
                        }

                    }
                }


                foreach (CategoriesInPosts catinpost in db.CategoriesInPosts.Where(p => p.FK_Posts == posts.Id).ToList())
                {
                    db.CategoriesInPosts.Remove(catinpost);

                }
                db.SaveChanges();

                string[] categories = Request.Form["Category"].Split(',');
                foreach (string itm in categories)
                {
                    int id = int.Parse(itm);
                    if (db.CategoriesInPosts.Where(p => p.FK_Posts == posts.Id && p.FK_Categories == id).Count() == 0)
                    {
                        CategoriesInPosts AddCategory = new CategoriesInPosts();
                        AddCategory.FK_Categories = id;
                        AddCategory.FK_Posts = posts.Id;
                        db.CategoriesInPosts.Add(AddCategory);
                    }
                }


                //Delete All Tags found for the post
                foreach (TagsInPosts tagini in db.TagsInPosts.Where(p => p.FK_Posts == posts.Id).ToList())
                {
                    db.TagsInPosts.Remove(tagini);

                }
                db.SaveChanges();                
                //is any tags have been givin
                if (posts.tags != null)
                {
                    //Tags
                    string TagTrim = posts.tags.ToString().Replace(" ", string.Empty).ToLower();
                    string[] tags = TagTrim.Split(',');
                    foreach (string tag in tags)
                    {
                        if (db.Tags.Where(p => p.Name.Contains(tag)).Count() == 0)
                        {
                            Tags addtag = new Tags();
                            addtag.Name = tag.ToLower();
                            db.Tags.Add(addtag);

                            TagsInPosts AddToItem = new TagsInPosts();
                            AddToItem.FK_Posts = posts.Id;
                            AddToItem.FK_Tag = addtag.Id;
                            db.TagsInPosts.Add(AddToItem);
                            db.SaveChanges();
                        }
                        else
                        {
                            //Get Id of Tag
                            int Tagid = db.Tags.Where(p => p.Name.Contains(tag)).FirstOrDefault().Id;
                            if (db.TagsInPosts.Where(p => p.FK_Tag == Tagid && p.FK_Posts == posts.Id).Count() == 0)
                            {
                                TagsInPosts AddToItem = new TagsInPosts();
                                AddToItem.FK_Posts = posts.Id;
                                AddToItem.FK_Tag = db.Tags.Where(p => p.Name.Contains(tag)).FirstOrDefault().Id;
                                db.TagsInPosts.Add(AddToItem);
                                db.SaveChanges();
                            }

                        }
                    }
                }
                return RedirectToAction("Index");
            }
            ViewBag.FK_Type = new SelectList(db.PostType, "Id", "Name", posts.FK_Type);
            ViewBag.FK_Status = new SelectList(db.Status, "Id", "Name", posts.FK_Status);
            
            return View(posts);
        }

        //
        // GET: /Projects/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Posts posts = db.Posts.Find(id);
            if (posts == null)
            {
                return HttpNotFound();
            }
            db.Posts.Remove(posts);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //
        // GET: /Projects/Publish/5

        public ActionResult Publish(int id = 0)
        {
            Posts posts = db.Posts.Find(id);
            if (posts == null)
            {
                return HttpNotFound();
            }
            if (posts.FK_Status == 1)
            {
                //Unpublish
                posts.FK_Status = 2;
            }
            else if (posts.FK_Status != 1)
            {
                //publish
                posts.FK_Status = 1;
            }
            db.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}