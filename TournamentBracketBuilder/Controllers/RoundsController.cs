using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentBracketBuilder.Data;
using TournamentBracketBuilder.Models;

namespace TournamentBracketBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoundsController : ControllerBase
    {
        private readonly BracketBuilderContext _context;

        public RoundsController(BracketBuilderContext context)
        {
            _context = context;
        }

        // PUT: api/RoundComplete/5
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Complete([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var round = await _context.Rounds
                .Include(r => r.Matches)
                    .ThenInclude(m => m.NextWinnersMatch)
                .Include(r => r.Matches)
                    .ThenInclude(m => m.NextLosersMatch)
                .SingleOrDefaultAsync(r => r.Id == id);

            if (round == null)
            {
                return BadRequest();
            }

            round.IsComplete = true;

            var rounds = new List<Round>();
            rounds.Add(round);

            UpdateMatches(round.Matches);

            if(round.RoundType == RoundType.GrandFinals)
            {
                var lastMatch = round.Matches.OrderBy(m => m.MatchNumber).FirstOrDefault();

                if(lastMatch.Winner == Slot.Two)
                {
                    var resetRound = new Round
                    {
                        RoundType = RoundType.Reset,
                        Matches = new List<Match>(),
                        RoundNumber = round.RoundNumber + 1,
                        TournamentId = round.TournamentId,
                        Name = "Grand Finals Reset"
                    };

                    var resetMatch = new Match {
                        MatchNumber = lastMatch.MatchNumber + 1,
                        CompetitorOneId = lastMatch.CompetitorOneId,
                        CompetitorTwoId = lastMatch.CompetitorTwoId
                    };
                    lastMatch.NextWinnersMatch = resetMatch;
                    lastMatch.NextWinnersMatchSlot = Slot.One;
                    lastMatch.NextLosersMatch = resetMatch;
                    lastMatch.NextLosersMatchSlot = Slot.Two;
                    _context.Entry(lastMatch).CurrentValues.SetValues(lastMatch);

                    resetRound.Matches.Add(resetMatch);

                    _context.Add(resetRound);
                    rounds.Add(resetRound);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoundExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(rounds);
        }

        private void UpdateMatches(IList<Match> matches)
        {
            for(var i = 0; i < matches.Count; i++)
            {
                int winnerId, loserId;
                if(matches[i].Winner == Slot.One)
                {
                    winnerId = (int)matches[i].CompetitorOneId;
                    loserId = (int)matches[i].CompetitorTwoId;
                }
                else
                {
                    winnerId = (int)matches[i].CompetitorTwoId;
                    loserId = (int)matches[i].CompetitorOneId;
                }

                if(matches[i].NextWinnersMatch != null)
                {
                    if(matches[i].NextWinnersMatchSlot == Slot.One)
                    {
                        matches[i].NextWinnersMatch.CompetitorOneId = winnerId;
                    }
                    else
                    {
                        matches[i].NextWinnersMatch.CompetitorTwoId = winnerId;
                    }

                    var original = _context.Matches.SingleOrDefault(x => x.Id == matches[i].NextWinnersMatch.Id);
                    _context.Entry(original).CurrentValues.SetValues(matches[i].NextWinnersMatch);
                }

                if (matches[i].NextLosersMatch != null)
                {
                    if (matches[i].NextLosersMatchSlot == Slot.One)
                    {
                        matches[i].NextLosersMatch.CompetitorOneId = loserId;
                    }
                    else
                    {
                        matches[i].NextLosersMatch.CompetitorTwoId = loserId;
                    }

                    var original = _context.Matches.SingleOrDefault(x => x.Id == matches[i].NextLosersMatch.Id);
                    _context.Entry(original).CurrentValues.SetValues(matches[i].NextLosersMatch);
                }
            }
            return;
        }

        private bool RoundExists(int id)
        {
            return _context.Rounds.Any(e => e.Id == id);
        }
    }
}