using System;
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
            var levelUpBar = levelUpLabel[0].ParentNode;
            var table = levelUpBar.ParentNode;
            var levelUpLearnerTable = table.ChildNodes[table.ChildNodes.GetNodeIndex(levelUpBar) + 2];
            var levelUpLearnerRows = levelUpLearnerTable.ChildNodes;
            var pokeList = GetPokemon(levelUpLearnerRows);
            return pokeList.Aggregate("", (current, pokemon) => current + " " + pokemon.Name);
        }

        private static IEnumerable<Pokemon> GetPokemon(HtmlNodeCollection rows)
        {
            var pokeList = new List<Pokemon>();
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
                pokeList.Add(poke);
            }
            return pokeList;
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