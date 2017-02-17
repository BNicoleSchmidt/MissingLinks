using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Ajax.Utilities;
using MissingLinks.Models;
using MissingLinks.Services;

namespace MissingLinks.Controllers
{
    public interface ILearnerHelper
    {
        List<Pokemon> GetLearners(HtmlDocument doc);
        List<string> GetResults(string poke, List<Pokemon> learners, string move);
        string GetApiResults(InputModel input);
    }

    public class LearnerHelper : ILearnerHelper
    {
        private readonly IPokeApiService _pokeApiService;

        public LearnerHelper(IPokeApiService pokeApiService)
        {
            _pokeApiService = pokeApiService;
        }

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
                    if (poke.Name.Contains("Mega ")) continue;
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
            }
            else
            {
                results.Add(poke + " doesn't seem to learn " + move + ". Did you spell something wrong?");
            }
            return results;
        }

        public string GetApiResults(InputModel input)
        {
            var lowerName = input.Pokemon.ToLower();
            var upperName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(lowerName);
            var lowerMove = input.Move.ToLower();
            var upperMove = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(lowerMove);
            if (lowerName.IsNullOrWhiteSpace()) return "I can't tell you about a Pokemon if you haven't given one!";
            if (lowerMove.IsNullOrWhiteSpace()) return "I can't tell you about a move if you haven't given one!";
            if (lowerName == "smeargle" && lowerMove != "sketch") return "Go sketch it, nerd.";
            var allLearners = _pokeApiService.GetPokemonWithMove(lowerMove);
            var currentPokemon = allLearners.FirstOrDefault(p => p.Name == lowerName);
            if (currentPokemon == null) return $"{upperName} doesn't seem to learn {upperMove}. Did you spell something wrong?";
            var learnedMove = currentPokemon.Moves.FirstOrDefault(m => m.Name == lowerMove);
            if (learnedMove.LevelUp || learnedMove.Machine || learnedMove.Tutor) return $"{upperName} learns {upperMove} on its own, or can be taught the move. No breeding necessary!";
            var compatible = allLearners.Where(x => x.EggGroups.Any(group => currentPokemon.EggGroups.Contains(@group)));
            var directCompatible =
                compatible.Where(
                    x =>
                        x != currentPokemon &&
                        (x.Moves.First(m => m.Name == lowerMove).LevelUp ||
                         x.Moves.First(m => m.Name == lowerMove).Tutor ||
                         x.Moves.First(m => m.Name == lowerMove).Machine));
            if(directCompatible.Any()) return directCompatible.Aggregate("Learn directly from: ", (current, learner) => current + " " + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(learner.Name));
            return "Dunno yet";
        }


        private void GetChain(string poke, List<Pokemon> learners, List<string> results)
        {
            var pokemon = learners.SingleOrDefault(x => string.Equals(x.Name, poke, StringComparison.InvariantCultureIgnoreCase));
            var compatible = GetCompatible(learners, pokemon);
            var directCompatible = GetDirectCompatible(compatible, pokemon);
            var indirectCompatible = compatible.Where(x => x != pokemon && x.Breed && !(x.LevelUp || x.Tutor || x.Machine));
            if (directCompatible.Any())
            {
                results.Add(directCompatible.Aggregate("Learn directly from: ", (current, learner) => current + " " + learner.Name));
            }
            else if (indirectCompatible.Any())
            {
                foreach (var learner in indirectCompatible)
                {
                    var currentCompatible = new List<Pokemon>();
                    currentCompatible.AddRange(GetCompatible(learners.ToList(), learner));
                    var currentDirect = GetDirectCompatible(currentCompatible, learner);
                    if (currentDirect.Any())
                    {
                        results.Add("Can learn from " + learner.Name + " who learns from " +
                                    currentDirect.FirstOrDefault().Name + " and possibly others.");
                    }
                }
//                results.Add(indirectCompatible.Aggregate("Learn from these, but must be bred onto them: ", (current, learner) => current + " " + learner.Name));
            }
        }

        

        private static IEnumerable<Pokemon> GetDirectCompatible(IEnumerable<Pokemon> compatible, Pokemon pokemon)
        {
            return compatible.Where(x => x != pokemon && (x.LevelUp || x.Tutor || x.Machine));
        }

        private static IEnumerable<Pokemon> GetCompatible(List<Pokemon> learners, Pokemon pokemon)
        {
            return learners.Where(x => x.EggGroups.Any(group => pokemon.EggGroups.Contains(@group)));
        }
    }
}