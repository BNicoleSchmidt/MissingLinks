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

            var learners = _learnerHelper.GetLearners(doc);

            ViewBag.LevelUps = learners.Where(x => x.LevelUp).ToArray();
            ViewBag.Eggs = learners.Where(x => x.Breed).ToArray();
            ViewBag.Tutors = learners.Where(x => x.Tutor).ToArray();
            ViewBag.Machines = learners.Where(x => x.Machine).ToArray();

            var poke = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.Pokemon);
            ViewBag.Move = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.Move);
            

            var results = _learnerHelper.GetResults(poke, learners, ViewBag.Move);
            ViewBag.Results = results.ToArray();
            return View();
        }
    }
}