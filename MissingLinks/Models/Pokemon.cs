using System.Collections.Generic;

namespace MissingLinks.Models
{
    public class Pokemon
    {
        public string Name { get; set; }
        public List<string> EggGroups { get; set; }

        public Pokemon()
        {
            EggGroups = new List<string>();
        }
    }
}