using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TournamentBracketBuilder.Models
{
    public enum Slot
    {
        One,
        Two
    }

    public class Match
    {
        public int Id { get; set; }
        public int MatchNumber { get; set; }
        public int RoundId { get; set; }
        public int? CompetitorOneId { get; set; }
        public int? CompetitorTwoId { get; set; }

        public int? NextWinnersMatchId { get; set; }
        public Slot? NextWinnersMatchSlot { get; set; }
        public int? NextLosersMatchId { get; set; }
        public Slot? NextLosersMatchSlot { get; set; }

        public int CompetitorOneScore { get; set; }
        public int CompetitorTwoScore { get; set; }

        public Slot? Winner { get; set; }

        public virtual Competitor CompetitorOne { get; set; }
        public virtual Competitor CompetitorTwo { get; set; }
        public virtual Round Round { get; set; }
        public virtual Match NextWinnersMatch { get; set; }
        public virtual Match NextLosersMatch { get; set; }
    }
}
