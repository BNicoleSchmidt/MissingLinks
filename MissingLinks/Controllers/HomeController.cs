using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HtmlAgilityPack;
using MissingLinks.Models;

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

        public ActionResult SummonVeekun(InputModel input)
        {
            string url = "http://www.veekun.com/dex/moves/" + input.Move;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            var result = doc.DocumentNode.SelectNodes("//*[@id=\"dex-page-name\"]")[0].InnerText;
            ViewBag.LevelUps = "By Level-up:" + GetLearners(doc, "level-up");
            ViewBag.Eggs = "By Breeding:" + GetLearners(doc, "egg");
            ViewBag.Tutors = "By Tutor:" + GetLearners(doc, "tutor");
            ViewBag.Machines = "By TM/HM:" + GetLearners(doc, "machine");

            ViewBag.Message = result;
            return View();
        }

        private string GetLearners(HtmlDocument doc, string method)
        {
            var levelUpLabel = doc.DocumentNode.SelectNodes("//tr[@id=\"pokemon:" + method + "\"]");
            if (levelUpLabel == null)
            {
                return " Nothing learns this move by this method.";
            }
            else
            {
                var levelUpBar = levelUpLabel[0].ParentNode;
                var table = levelUpBar.ParentNode;
                var levelUpLearnerTable = table.ChildNodes[table.ChildNodes.GetNodeIndex(levelUpBar) + 2];
                var levelUpLearnerRows = levelUpLearnerTable.ChildNodes;
                var learners = new List<string>();
                learners.AddRange(GetNames(levelUpLearnerRows));
                return learners.Aggregate("", (current, pokemon) => current + (" " + pokemon));
            }
        }

        private static IEnumerable<string> GetNames(HtmlNodeCollection rows)
        {
            return from row in rows where row.Name == "tr" from col in row.ChildNodes where col.InnerHtml.Contains("/dex/pokemon") select col.InnerText;
        }
    }
}