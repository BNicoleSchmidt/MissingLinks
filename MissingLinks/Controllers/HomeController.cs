using System;
using System.Collections.Generic;
using System.Globalization;
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

            var moveName = doc.DocumentNode.SelectNodes("//*[@id=\"dex-page-name\"]")[0].InnerText;
            ViewBag.Message = moveName;

            var learners = GetLearners(doc);
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

            string result;
            if (string.Equals(poke, "Smeargle", StringComparison.InvariantCultureIgnoreCase))
            {
                result = "Go sketch it, nerd.";
            }
            else if (learners.Any(x => string.Equals(x.Name, poke, StringComparison.InvariantCultureIgnoreCase) && (x.LevelUp || x.Tutor || x.Machine)))
            {
                result = poke + " learns " + move + " on its own, or can be taught the move. No breeding necessary!";
            }
            else if (learners.Any(x => string.Equals(x.Name, poke, StringComparison.InvariantCultureIgnoreCase) && x.Breed))
            {
                result = GetChain(poke, move, learners);
                //result = poke + " learns " + move + " somehow.";
            }
            else
            {
                result = poke + " doesn't seem to learn " + move + ". Did you spell something wrong?";
            }
            ViewBag.Result = result;
            return View();
        }

        private string GetChain(string poke, string move, List<Pokemon> learners)
        {
            var pokemon = learners.SingleOrDefault(x => string.Equals(x.Name, poke, StringComparison.InvariantCultureIgnoreCase));
            var compatible = learners.Where(x => x.EggGroups.Any(group => pokemon.EggGroups.Contains(group)));
            return compatible.Where(x => x != pokemon).Aggregate("", (current, learner) => current + " " + learner.Name);
        }

        private List<Pokemon> GetLearners(HtmlDocument doc)
        {
            var learners = new List<Pokemon>();
            foreach (var method in new[] {"level-up", "egg", "tutor", "machine"})
            {
                var methodLabel = doc.DocumentNode.SelectNodes("//tr[@id=\"pokemon:" + method + "\"]");
                if (methodLabel == null)
                {
                    continue;
                }
                var methodHeader = methodLabel[0].ParentNode;
                var table = methodHeader.ParentNode;
                var methodTable = table.ChildNodes[table.ChildNodes.GetNodeIndex(methodHeader) + 2];
                var methodLearnerRows = methodTable.ChildNodes;
                GetLearners(methodLearnerRows, learners, method);
            }
            return learners;
        }

        private static void GetLearners(HtmlNodeCollection rows, List<Pokemon> learners, string method)
        {
            foreach (var row in rows)
            {
                if (row.Name != "tr") continue;
                var poke = new Pokemon();
                foreach (var col in row.ChildNodes)
                {
                    if (col.InnerHtml.Contains("/dex/pokemon")) poke.Name = col.InnerText;
                    if (col.Attributes["class"] == null || !col.Attributes["class"].Value.Contains("egg-group")) continue;
                    SetEggGroups(col, poke);
                }
                if (learners.Any(x => x.Name == poke.Name))
                {
                    switch(method)
                    {
                        case "level-up":
                            learners.SingleOrDefault(x => x.Name == poke.Name).LevelUp = true;
                            break;
                        case "egg":
                            learners.SingleOrDefault(x => x.Name == poke.Name).Breed = true;
                            break;
                        case "tutor":
                            learners.SingleOrDefault(x => x.Name == poke.Name).Tutor = true;
                            break;
                        case "machine":
                            learners.SingleOrDefault(x => x.Name == poke.Name).Machine = true;
                            break;
                    }
                }
                else
                {
                    switch (method)
                    {
                        case "level-up":
                            poke.LevelUp = true;
                            learners.Add(poke);
                            break;
                        case "egg":
                            poke.Breed = true;
                            learners.Add(poke);
                            break;
                        case "tutor":
                            poke.Tutor = true;
                            learners.Add(poke);
                            break;
                        case "machine":
                            poke.Machine = true;
                            learners.Add(poke);
                            break;
                    }
                }
            }
        }
        
        private static void SetEggGroups(HtmlNode col, Pokemon poke)
        {
            var eggGroups = col.InnerHtml.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var eggGroup in eggGroups)
            {
                if (!string.IsNullOrWhiteSpace(eggGroup) && !eggGroup.Contains("<br>"))
                {
                    poke.EggGroups.Add(eggGroup.Trim());
                }
            }
        }
    }
}