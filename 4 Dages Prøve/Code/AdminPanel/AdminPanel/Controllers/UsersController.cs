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
using WebMatrix.WebData;
using System.Data.Entity.Validation;

namespace AdminPanel.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class UsersController : Controller
    {
        private Entities db = new Entities();

        //
        // GET: /Users/

        public ActionResult Index()
        {
            return View(db.UserProfile.ToList());
        }

        //
        // GET: /Users/Details/5

        public ActionResult Details(int id = 0)
        {
            UserProfile userprofile = db.UserProfile.Find(id);
            if (userprofile == null)
            {
                return HttpNotFound();
            }
            return View(userprofile);
        }

        //
        // GET: /Users/Create

        public ActionResult Create()
        {
            return View(new UserProfile());
        }

        //
        // POST: /Users/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserProfile userprofile)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    WebSecurity.CreateUserAndAccount(userprofile.UserName, userprofile.Password);
                    Roles.AddUserToRole(userprofile.UserName, "Administrator");
                    
                    UserProfile ItmtoEdit = db.UserProfile.Where(p => p.UserName == userprofile.UserName).FirstOrDefault();
                    ItmtoEdit.FirstName = userprofile.FirstName;
                    ItmtoEdit.LastName = userprofile.LastName;
                    ItmtoEdit.Email = userprofile.Email;

                    db.SaveChanges();

                }
                catch (DbEntityValidationException e)
                {
                    throw e;
                }
                return RedirectToAction("Index");
            }

            return View(userprofile);
        }

        //
        // GET: /Users/Edit/5

        public ActionResult Edit(int id = 0)
        {
            UserProfile userprofile = db.UserProfile.Find(id);
            if (userprofile == null)
            {
                return HttpNotFound();
            }
            return View(userprofile);
        }

        //
        // POST: /Users/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserProfile userprofile)
        {
            if (ModelState.IsValid)
            {
                


                if (userprofile.Password != null)
                {
                    MembershipUser user;
                    user = Membership.GetUser(userprofile.UserName, false);
                    user.ChangePassword(user.ResetPassword(), "123456");
                    ViewBag.ErrorMessage = "Password Updated";
                }

                db.Entry(userprofile).State = EntityState.Modified;
                db.SaveChanges();
                
            }
            return View(userprofile);
        }

        //
        // GET: /Users/Delete/5

        public ActionResult Delete(int id = 0)
        {
            UserProfile userprofile = db.UserProfile.Find(id);
            if (userprofile == null)
            {
                return HttpNotFound();
            }
            ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(userprofile.UserName); // deletes record from webpages_Membership table
            ((SimpleMembershipProvider)Membership.Provider).DeleteUser(userprofile.UserName, true); // deletes record from UserProfile table

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}