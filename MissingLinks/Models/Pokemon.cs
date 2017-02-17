using System.Collections.Generic;

namespace MissingLinks.Models
{
    public class Pokemon
    {
        public string Name { get; set; }
        public List<string> EggGroups { get; set; }
        public bool LevelUp { get; set; }
        public bool Breed { get; set; }
        public bool Tutor { get; set; }
        public bool Machine { get; set; }

        public Pokemon()
        {
            EggGroups = new List<string>();
        }
    }

    public class ApiPokemon
    {
        public string Name { get; set; }
        public List<string> EggGroups { get; set; }
        public List<Move> Moves { get; set; }
        public int? EvolvesFrom { get; set; }
        public int? EvolvesTo { get; set; }
        public bool MaleOnly { get; set; }
        public bool FemaleOnly { get; set; }
        public bool Genderless { get; set; }

        public ApiPokemon()
        {
            EggGroups = new List<string>();
            Moves = new List<Move>();
        }
    }
}