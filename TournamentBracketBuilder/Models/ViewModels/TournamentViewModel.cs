using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TournamentBracketBuilder.Models.ViewModels
{
    public class TournamentViewModel
    {
        public Tournament Tournament { get; set; }
        public Dictionary<int, Competitor> Competitors { get; set; }
        public List<Round> Rounds { get; set; }
        public Dictionary<int, Match> Matches { get; set; }
    }
}
