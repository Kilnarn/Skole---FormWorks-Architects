using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using UmbracoMVC.Models;

namespace UmbracoMVC.Controllers
{
    

    public class ContactSurfaceController : SurfaceController
    {
        public ActionResult Index()
        {
            Contact test = new Contact();
            test.ToEmail = CurrentPage.GetProperty("email",false).ToString();
            return View(new Contact());
        }

        [HttpPost]
        public ActionResult SendMail(Contact form)
        {
            
            if (!ModelState.IsValid)
            {
                TempData.Add("Alert", "alert");
                TempData.Add("Status", "Der opstod en fejl udfyld venligst alle felter korrekt.");
                return RedirectToCurrentUmbracoPage();
            }

            if (ModelState.IsValid)
            {
                //Update your SMTP server credentials
                using (var client = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential("twin159@gmail.com", "tdpjruobgczzzrrh"),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                })
                {
                    var mail = new MailMessage();
                    mail.To.Add("twin159@gmail.com"); // Update your email address
                    mail.From = new MailAddress(form.Email, form.Name);
                    
                    mail.Subject = String.Format("Request From Contact by: {0}",form.Name);
                    mail.Body = form.Message;
                    mail.IsBodyHtml = true;
                    try
                    {
                        client.Send(mail);
                        TempData.Add("Alert", "success");
                        TempData.Add("Status", "Din besked er nu sendt, og vil blive besvaret hurtigst muligt");
                              
                            
                    }
                    catch (Exception)
                    {
                        throw;
                       
                    }
                }
            }
            return RedirectToCurrentUmbracoPage();
        }
    }
}
