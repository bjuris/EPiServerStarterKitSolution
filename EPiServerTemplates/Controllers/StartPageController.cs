using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;

namespace EPiServerTemplates.Models.Pages
{
    public class StartPageController : PageController<EPiServerTemplates.Models.Pages.StartPage>
    {
        public ActionResult Index(StartPage currentPage)
        {
            ViewBag.Title = currentPage.Name;

            return View(currentPage);
        }
    }
}