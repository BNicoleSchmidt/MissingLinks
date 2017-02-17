using System.Collections.Generic;
using HtmlAgilityPack;
using MissingLinks.Controllers;
using MissingLinks.Models;

namespace MissingLinks.Services
{
    public class VeekunService
    {
        public List<Pokemon> GetLearners(InputModel input)
        {
            var learnerHelper = new LearnerHelper(null);
            var url = "http://www.veekun.com/dex/moves/" + input.Move;
            var web = new HtmlWeb();
            var doc = web.Load(url);
            return learnerHelper.GetLearners(doc) ?? new List<Pokemon>();
        }
    }
}