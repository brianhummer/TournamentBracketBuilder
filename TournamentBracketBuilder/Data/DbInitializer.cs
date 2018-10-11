using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TournamentBracketBuilder.Models;

namespace TournamentBracketBuilder.Data
{
    public class DbInitializer
    {
        public static void Initialize(BracketBuilderContext context)
        {
            context.Database.EnsureCreated();

            if (context.Tournaments.Any())
            {
                return;
            }

            context.SaveChanges();
        }
    }
}