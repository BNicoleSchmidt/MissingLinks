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
}