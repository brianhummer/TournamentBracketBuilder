using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TournamentBracketBuilder.Models
{
    public class Competitor
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public int Seed { get; set; }
        [StringLength(32)]
        public string Name { get; set; }

        public virtual Tournament Tournament { get; set; }

    }
}
