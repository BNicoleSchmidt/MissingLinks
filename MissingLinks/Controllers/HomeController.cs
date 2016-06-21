using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;

namespace MissingLinks.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult SummonVeekun()
        {
            string result;
            string Url = "http://www.veekun.com";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(Url);

            result = doc.DocumentNode.SelectNodes("//*[@id=\"title\"]")[0].InnerText;
            ViewBag.Message = result;
            return View();
        }
    }
}