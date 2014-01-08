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
    public class SettingsController : Controller
    {
        private Entities db = new Entities();
        //
        // GET: /Settings/

        public ActionResult Index()
        {
            if (db.General.Count() == 0)
            {
                ViewBag.SubmitButton = "Create";
                return View(new General());
            }else{

                var item = db.General.FirstOrDefault();

                ViewBag.SubmitButton = "Save";
                return View(item);
            }
        }

        //
        // POST: /Settings/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Index(General general)
        {
            ViewBag.SubmitButton = "Save";

            General general2 = db.General.Where(p => p.Id == general.Id).FirstOrDefault();
            
            if (db.General.Count() == 0)
            {
                db.General.Add(general);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                if (ModelState.IsValid)
                {

                    general2.Address = general.Address;
                    general2.Email = general.Email;
                    general2.Phone = general.Phone;
                    general2.SiteName = general.SiteName;
                    general2.FK_City = db.City.Where(p => p.ZipCode == general.City.ZipCode).FirstOrDefault().Id;
                    //If Zipcodes does not exsist return to View and display error message
                    if (db.City.Where(p => p.ZipCode == general.City.ZipCode).Count() == 0)
                    {
                        ViewBag.ErrorMessage = "Zip Code was not found";
                        return View(general);
                        
                    }
                    //Get ID Of Zipcode;
                    //item.CityName = db.City.Where(p => p.ZipCode == item.City.ZipCode).FirstOrDefault().CityName;
                    //general.City = db.City.Where(p => p.ZipCode == general.City.ZipCode).FirstOrDefault();
                    //general.FK_City = db.City.FirstOrDefault().Id;
                    //db.Entry(general).State = EntityState.Modified;        
                    db.SaveChanges();
                }
            
            }

            return View(general2);
        }

    }
}
