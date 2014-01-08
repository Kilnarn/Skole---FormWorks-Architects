using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DA;
using System.Web.Security;
using AdminPanel.Filters;

namespace AdminPanel.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class PagesController : Controller
    {
        private DA.Entities db = new DA.Entities();
        
        //
        // GET: /Pages/

        public ActionResult Index()
        {
            var pages = db.Pages.Include(p => p.Pages2).Include(p => p.Status);

            return View(pages.ToList());
        }

        //
        // GET: /Pages/Create

        public ActionResult Create()
        {
            

            ViewBag.FK_Parent = new SelectList(db.Pages, "Id", "Name");
            
            
            ViewBag.FK_Status = new SelectList(db.Status, "Id", "Name");
            
            var Templates = from DA.Helpers.Templates s in Enum.GetValues(typeof(DA.Helpers.Templates)) select new { ID = (int)s, Name = s.ToString()};
            ViewData["PageTemplate"] = new SelectList(Templates, "ID", "Name");

            return View();
        }
        //
        // POST: /Pages/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Pages pages)
        {
            pages.CreatedDate = DateTime.Now;
            if (db.Pages.Count() != 0)
            {
                pages.SortIndex = db.Pages.OrderByDescending(p => p.SortIndex).FirstOrDefault().SortIndex + 1;
            }
            else
            {
                pages.SortIndex = 1;
            }
            pages.FK_User = (int)Membership.GetUser().ProviderUserKey;

            if (pages.FK_Parent != null)
            {
                pages.indentlevel = db.Pages.Where(p => p.Id == pages.FK_Parent).FirstOrDefault().indentlevel + 1;
                pages.SortIndex = db.Pages.Where(p => p.Id == pages.FK_Parent).FirstOrDefault().SortIndex + 1;
            }
            else
            {
                pages.indentlevel = 0;
            }
            
            pages.PageTemplate = (int)Enum.Parse(typeof(DA.Helpers.Templates), pages.PageTemplate.ToString());
            db.Pages.Add(pages);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //
        // GET: /Pages/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Pages pages = db.Pages.Find(id);
            if (pages == null)
            {
                return HttpNotFound();
            }

            SelectList Parent = new SelectList(db.Pages, "Id", "Name");



            ViewBag.FK_Parent = Parent.Where(p => p.Value != pages.Id.ToString()).ToList(); //new SelectList(db.Pages, "Id", "Name", pages.FK_Parent);
            ViewBag.FK_Status = new SelectList(db.Status, "Id", "Name", pages.FK_Status);

            var Templates = from DA.Helpers.Templates s in Enum.GetValues(typeof(DA.Helpers.Templates)) select new { ID = (int)s, Name = s.ToString() };
            ViewData["PageTemplate"] = new SelectList(Templates, "ID", "Name", pages.PageTemplate);

            return View(pages);
        }

        //
        // POST: /Pages/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Pages pages)
        {
            if (ModelState.IsValid)
            {
//                pages.CreatedDate = (DateTime)pages.CreatedDate;
                pages.FK_User = (int)Membership.GetUser().ProviderUserKey;
                db.Entry(pages).State = EntityState.Modified;
                pages.PageTemplate = (int)Enum.Parse(typeof(DA.Helpers.Templates), pages.PageTemplate.ToString());


                if (pages.FK_Parent != null)
                {
                    pages.indentlevel = db.Pages.Where(p => p.Id == pages.FK_Parent).FirstOrDefault().indentlevel + 1;
                    pages.SortIndex = db.Pages.Where(p => p.Id == pages.FK_Parent).FirstOrDefault().SortIndex;
                }
                else
                {
                    pages.indentlevel = 0;
                }


                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FK_Parent = new SelectList(db.Pages, "Id", "Name", pages.FK_Parent);
            ViewBag.FK_Status = new SelectList(db.Status, "Id", "Name", pages.FK_Status);
            return View(pages);
        }

        //
        // GET: /Pages/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Pages pages = db.Pages.Find(id);
            if (pages == null)
            {
                return HttpNotFound();
            }
            else
            {
                db.Pages.Remove(pages);
                db.SaveChanges();
                return RedirectToAction("Index");

            }
            
        }

        //
        // GET: /Pages/Publish/5

        public ActionResult Publish(int id = 0)
        {
            Pages pages = db.Pages.Find(id);
            if (pages == null)
            {
                return HttpNotFound();
            }
            if (pages.FK_Status == 1)
            {
                //Unpublish
                pages.FK_Status = 2;
            }
            else if (pages.FK_Status != 1)
            {
                //publish
                pages.FK_Status = 1;
            }
            db.SaveChanges();

            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}