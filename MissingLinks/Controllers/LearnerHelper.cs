using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using MissingLinks.Models;

namespace MissingLinks.Controllers
{
    public class LearnerHelper
    {
        public List<Pokemon> GetLearners(HtmlDocument doc)
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
                foreach (var row in methodLearnerRows)
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
                        switch (method)
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
            return learners;
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

        public List<string> GetResults(string poke, List<Pokemon> learners, string move)
        {
            var results = new List<string>();
            if (string.Equals(poke, "Smeargle", StringComparison.InvariantCultureIgnoreCase))
            {
                results.Add("Go sketch it, nerd.");
            }
            else if (learners.Any(x =>string.Equals(x.Name, poke, StringComparison.InvariantCultureIgnoreCase) && (x.LevelUp || x.Tutor || x.Machine)))
            {
                results.Add(poke + " learns " + move + " on its own, or can be taught the move. No breeding necessary!");
            }
            else if (learners.Any(x => string.Equals(x.Name, poke, StringComparison.InvariantCultureIgnoreCase) && x.EggGroups.Contains("Undiscovered")))
            {
                results.Add(poke + " can't breed. I don't know how to get a move onto this. (If this is a baby form of a Pokemon that DOES breed, stay tuned for possible upgrades to this system.)");
            }
            else if (learners.Any(x => string.Equals(x.Name, poke, StringComparison.InvariantCultureIgnoreCase) && x.Breed))
            {
                GetChain(poke, learners, results);
                //result = poke + " learns " + move + " somehow.";
            }
            else
            {
                results.Add(poke + " doesn't seem to learn " + move + ". Did you spell something wrong?");
            }
            return results;
        }

        private void GetChain(string poke, List<Pokemon> learners, List<string> results)
        {
            var pokemon = learners.SingleOrDefault(x => string.Equals(x.Name, poke, StringComparison.InvariantCultureIgnoreCase));
            var compatible = learners.Where(x => x.EggGroups.Any(group => pokemon.EggGroups.Contains(group)));
            if (compatible.Any(x => x != pokemon && (x.LevelUp || x.Tutor || x.Machine)))
            {
                results.Add(compatible.Where(x => x != pokemon && (x.LevelUp || x.Tutor || x.Machine)).Aggregate("Learn directly from: ", (current, learner) => current + " " + learner.Name));
            }
            if (compatible.Any(x => x != pokemon && x.Breed && !(x.LevelUp || x.Tutor || x.Machine)))
            {
                results.Add(compatible.Where(x => x != pokemon && x.Breed && !(x.LevelUp || x.Tutor || x.Machine)).Aggregate("Learn from these, but must be bred onto them: ", (current, learner) => current + " " + learner.Name));
            }
        }
    }
}