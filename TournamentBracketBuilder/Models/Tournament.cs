using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TournamentBracketBuilder.Models
{
    public class Tournament
    {
        public int Id { get; set; }
        [StringLength(64)]
        public string Name { get; set; }
        public bool IsDoubleElim { get; set; }


        public virtual IList<Round> Rounds { get; set; }
        public virtual IList<Competitor> Competitors { get; set; }
    }
}
