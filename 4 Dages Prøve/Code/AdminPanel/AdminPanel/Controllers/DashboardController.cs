using AdminPanel.Filters;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminPanel.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class DashboardController : Controller
    {

        private Entities db = new Entities();

        //
        // GET: /Dashboard/

        public ActionResult Index()
        {

            GetInformation vm = new GetInformation();

            vm.GetPages = from page in db.Pages select page;
            vm.GetPosts = from allposts in db.Posts select allposts;
            vm.GetTags  = from tag in db.Tags select tag;
            vm.GetCateGories = from cat in db.Categories select cat;
            
            return View(vm);
        }

    }
}