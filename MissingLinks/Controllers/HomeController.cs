using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using HtmlAgilityPack;
using MissingLinks.Models;

namespace MissingLinks.Controllers
{
    public class HomeController : Controller
    {
        private readonly LearnerHelper _learnerHelper = new LearnerHelper();

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

            var moveName = doc.DocumentNode.SelectNodes("//*[@id=\"dex-page-name\"]")[0].InnerText;
            ViewBag.Message = moveName;

            var learners = _learnerHelper.GetLearners(doc);
            string levelUps = learners.Any(x => x.LevelUp)
                ? learners.Where(x => x.LevelUp).Aggregate("", (current, pokemon) => current + " " + pokemon.Name)
                : " Nothing learns this move through this method.";
            string breed = learners.Any(x => x.Breed)
                ? learners.Where(x => x.Breed).Aggregate("", (current, pokemon) => current + " " + pokemon.Name)
                : " Nothing learns this move through this method.";
            string tutor = learners.Any(x => x.Tutor)
                ? learners.Where(x => x.Tutor).Aggregate("", (current, pokemon) => current + " " + pokemon.Name)
                : " Nothing learns this move through this method.";
            string machine = learners.Any(x => x.Machine)
                ? learners.Where(x => x.Machine).Aggregate("", (current, pokemon) => current + " " + pokemon.Name)
                : " Nothing learns this move through this method.";

            ViewBag.LevelUps = "By Level-up:" + levelUps;
            ViewBag.Eggs = "By Breeding:" + breed;
            ViewBag.Tutors = "By Tutor:" + tutor;
            ViewBag.Machines = "By TM/HM:" + machine;

            var poke = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.Pokemon);
            var move = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.Move);
            
            var results = _learnerHelper.GetResults(poke, learners, move);
            ViewBag.Results = results.ToArray();
            return View();
        }
    }
}