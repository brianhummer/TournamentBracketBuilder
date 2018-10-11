using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TournamentBracketBuilder.Models;

namespace TournamentBracketBuilder.Data
{
    public class BracketBuilderContext : DbContext
    {
        public BracketBuilderContext(DbContextOptions<BracketBuilderContext> options) : base(options)
        {

        }

        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Competitor> Competitors { get; set; }
        public DbSet<Round> Rounds { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.NextWinnersMatch)
                .WithMany()
                .HasPrincipalKey("Id")
                .HasForeignKey("NextWinnersMatchId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.NextLosersMatch)
                .WithMany()
                .HasPrincipalKey("Id")
                .HasForeignKey("NextLosersMatchId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
