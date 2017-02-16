using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using HtmlAgilityPack;
using MissingLinks.Models;
using MissingLinks.Services;
using Newtonsoft.Json.Linq;

namespace MissingLinks.Controllers
{
    public class HomeController : Controller
    {
        private readonly VeekunService _veekunService = new VeekunService();
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

        public ViewResult SummonVeekun(InputModel input)
        {
            var learners = _veekunService.GetLearners(input);
            var testlearners = PokeApiService.GetAllPokemon();

            ViewBag.Test = testlearners;

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