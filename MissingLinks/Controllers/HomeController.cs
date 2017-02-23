using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using MissingLinks.Models;

namespace MissingLinks.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILearnerHelper _learnerHelper;

        public HomeController(ILearnerHelper learnerHelper)
        {
            _learnerHelper = learnerHelper;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ViewResult SummonVeekun(InputModel input)
        {
            ViewBag.LevelUps = _learnerHelper.GetLevelUpLearners(input.Move).ToArray();
            ViewBag.Eggs = _learnerHelper.GetEggLearners(input.Move).ToArray();
            ViewBag.Tutors = _learnerHelper.GetTutorLearners(input.Move).ToArray();
            ViewBag.Machines = _learnerHelper.GetMachineLearners(input.Move).ToArray();
            ViewBag.Move = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.Move);
            ViewBag.Results = _learnerHelper.GetApiResults(input);
            return View();
        }
    }
}