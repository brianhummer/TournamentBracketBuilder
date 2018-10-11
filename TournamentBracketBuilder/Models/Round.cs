using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TournamentBracketBuilder.Models
{
    public enum RoundType
    {
        Winners,
        Losers,
        GrandFinals,
        Reset
    }

    public class Round
    {
        public int Id { get; set; }
        public int RoundNumber { get; set; }
        public string Name { get; set; }
        public int TournamentId { get; set; }
        public bool IsComplete { get; set; }
        public RoundType? RoundType { get; set; }

        public virtual IList<Match> Matches { get; set; }
        public virtual Tournament Tournament { get; set; }
    }
}
