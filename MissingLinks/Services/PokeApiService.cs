using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MissingLinks.Models;
using Newtonsoft.Json.Linq;

namespace MissingLinks.Services
{
    public static class PokeApiService
    {
        private static List<ApiPokemon> _allPokemon;
        private static readonly List<string> VersionExclusions = new List<string>
        {
            "colosseum", "xd", "red-blue", "yellow", "gold-silver", "crystal"
        };


        public static async void CacheAllData()
        {
            _allPokemon = new List<ApiPokemon>();
            var client = new HttpClient();

            for (var i = 1; i <= 11; i++)
            {
                var rawPokemon = JObject.Parse(await client.GetStringAsync("http://pokeapi.co/api/v2/pokemon/" + i));
                var rawSpecies = JObject.Parse(await client.GetStringAsync((string)rawPokemon["species"]["url"]));

                var pokemon = new ApiPokemon
                {
                    Name = (string)rawPokemon["name"],
                    Moves = new List<Move>(),
                    EggGroups = new List<string>(),
                    FemaleOnly = (int)rawSpecies["gender_rate"] == 8,
                    MaleOnly = (int)rawSpecies["gender_rate"] == 0,
                    Genderless = (int)rawSpecies["gender_rate"] == -1
                };

                //                var evoChain = rawSpecies["evolution_chain"];
                //                if (evoChain != null)
                //                {
                //                    var rawEvoChain = JObject.Parse(await client.GetStringAsync((string)evoChain["url"]));
                //                }

                foreach (var eggGroup in rawSpecies["egg_groups"])
                {
                    pokemon.EggGroups.Add((string)eggGroup["name"]);
                }

                foreach (var rawMove in rawPokemon["moves"])
                {
                    var move = new Move { Name = (string)rawMove["name"], Versions = new List<string>() };
                    foreach (var rawVersionGroup in rawMove["version_group_details"])
                    {
                        var version = (string)rawVersionGroup["version_group"]["name"];

                        if (VersionExclusions.Contains(version)) continue;

                        var method = (string)rawVersionGroup["move_learn_method"]["name"];
                        if (method == "level-up") move.LevelUp = true;
                        if (method == "egg") move.Breed = true;
                        if (method == "machine") move.Machine = true;
                        if (method == "tutor") move.Tutor = true;
                        if (!move.Versions.Contains(version)) move.Versions.Add(version);
                    }
                    if (move.Versions.Any()) pokemon.Moves.Add(move);
                }
                _allPokemon.Add(pokemon);
            }
        }

        public static List<ApiPokemon> GetAllPokemon()
        {
            return _allPokemon;
        }
    }
}