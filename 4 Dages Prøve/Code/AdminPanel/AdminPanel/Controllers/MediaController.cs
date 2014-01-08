using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using System.IO;
using System.Web.Security;
using System.Drawing;

namespace AdminPanel.Controllers
{
    public class MediaController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /Media/

        public ActionResult Index()
        {
            var media = db.Media.Include(m => m.FileType).Include(m => m.UserProfile);

            return View(media.ToList());
        }

        //
        // GET: /Media/Details/5

        public ActionResult Details(int id = 0)
        {
            Media media = db.Media.Find(id);
            if (media == null)
            {
                return HttpNotFound();
            }
            return View(media);
        }

        //
        // GET: /Media/Create

        public ActionResult Create()
        {
            

            return View(new Media());
        }

        //
        // POST: /Media/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Media media)
        {
            if (ModelState.IsValid)
            {
                media.DateCreated = DateTime.Now;
                media.Fk_User = (int)Membership.GetUser().ProviderUserKey;
                media.Path = "none";


                if (media.File != null)
                {
                    // Generate a filename to store the file on the server
                    var Extension = Path.GetExtension(media.File.FileName);
                    
                    //Get image Information
                    var ImageInfo = Image.FromStream(media.File.InputStream, true, true);
                    media.Height = ImageInfo.Height;
                    media.Width = ImageInfo.Width;


                    //Check if file is valid if it it add the the id to the FK Else return view with error message
                    if (db.FileType.Where(p => p.Name == Extension).Count() > 0)
                    {
                        media.Fk_FileType = db.FileType.Where(p => p.Name == Extension).FirstOrDefault().Id;         
                    }
                    else
                    {
                        ViewBag.Message = "Image file type not supported. Supported files (Jpg, png, gif)";
                        return View(media);
                    }

                    if (media.Name == null)
                    {
                        media.Name = media.File.FileName.Substring(0, media.File.FileName.Length - 4);
                    }
                }
                
                //Add the Media to db except the real path.
                db.Media.Add(media);
                db.SaveChanges();

                if (media.File != null)
                {
                    var Extension = Path.GetExtension(media.File.FileName);
                    var fileName = "Media-" + media.Id + "-" + media.File.FileName;
                    var uploadinpath = Path.Combine(Server.MapPath("~/Content/Media"), fileName);

                    media.Path = fileName;
                    media.File.SaveAs(uploadinpath);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(media);
        }

        //
        // GET: /Media/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Media media = db.Media.Find(id);
            if (media == null)
            {
                return HttpNotFound();
            }
            ViewBag.Fk_FileType = new SelectList(db.FileType, "Id", "Name", media.Fk_FileType);
            ViewBag.Fk_User = new SelectList(db.UserProfile, "UserId", "UserName", media.Fk_User);
            return View(media);
        }

        //
        // POST: /Media/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Media media)
        {            
            if (!ModelState.IsValid)
            {
                // the user didn't upload any file =>
                // render the same view again in order to display the error message
                return View(media);
            }
            // GET ITEM TO EDIT
            DA.Media itmToEdit = db.Media.Where(p => p.Id == media.Id).FirstOrDefault();
            //If media item has been found
            if (db.Media.Where(p => p.Id == media.Id).Count() != 0)
            {

                itmToEdit.Caption = media.Caption;
                itmToEdit.DateCreated = DateTime.Now;
                itmToEdit.Fk_User = (int)Membership.GetUser().ProviderUserKey;
                itmToEdit.Name = media.Name;
                itmToEdit.SEO_Description = media.SEO_Description;
                itmToEdit.SEO_KeyWords = media.SEO_KeyWords;

                //if there is a new file.
                if (media.File != null)
                {
                    // Generate a filename to store the file on the server
                    string Extension = Path.GetExtension(media.File.FileName);
                    string fileName = "Media-" + media.Id + "-" + media.File.FileName;
                    string uploadinpath = Path.Combine(Server.MapPath("~/Content/Media"), fileName);

                    //Check if file type is valid
                    if (db.FileType.Where(p => p.Name == Extension).Count() > 0)
                    {

                        itmToEdit.Fk_FileType = db.FileType.Where(p => p.Name == Extension).FirstOrDefault().Id;
                        media.Path = fileName.ToString();
                    }
                    else
                    {
                        ViewBag.Message = "Image file type not supported. Supported files (Jpg, png, gif)";

                        db.SaveChanges();
                        return View(media);
                    }

                    //Save Path and file at the server
                    itmToEdit.Path = fileName.ToString();
                    media.File.SaveAs(uploadinpath);

                    //Get file information
                    var ImageInfo = Image.FromStream(media.File.InputStream, true, true);
                    itmToEdit.Height = ImageInfo.Height;
                    itmToEdit.Width = ImageInfo.Width;

                    media.Height = ImageInfo.Height;
                    media.Width = ImageInfo.Width;
                }

                db.SaveChanges();
                return View(media);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        //
        // GET: /Media/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Media media = db.Media.Find(id);
            if (media == null)
            {
                return HttpNotFound();
            }
            db.Media.Remove(media);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}