using System.Collections.Generic;

namespace MissingLinks.Models
{
    public class Move
    {
        public string Name { get; set; }
        public List<string> Versions { get; set; }
        public bool LevelUp { get; set; }
        public bool Breed { get; set; }
        public bool Tutor { get; set; }
        public bool Machine { get; set; }
    }
}