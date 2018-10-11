using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TournamentBracketBuilder.Data;
using TournamentBracketBuilder.Models;
using TournamentBracketBuilder.Models.ViewModels;

namespace TournamentBracketBuilder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentsController : ControllerBase
    {
        private readonly BracketBuilderContext _context;

        public TournamentsController(BracketBuilderContext context)
        {
            _context = context;
        }

        // GET: api/Tournaments
        [HttpGet]
        public IEnumerable<Tournament> GetTournaments()
        {
            return _context.Tournaments.Include(t => t.Competitors);
        }

        // GET: api/Tournaments/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTournament([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var viewModel = new TournamentViewModel();

            var tournament = await _context.Tournaments
                .Include(t => t.Competitors)
                .Include(t => t.Rounds)
                        .ThenInclude(r => r.Matches)
                .SingleOrDefaultAsync(t => t.Id == id);

            tournament.Rounds = tournament.Rounds.OrderBy(r => r.RoundNumber).ToList();
            for(var i = 0; i < tournament.Rounds.Count; i++)
            {
                tournament.Rounds[i].Matches = tournament.Rounds[i].Matches.OrderBy(m => m.MatchNumber).ToList();
            }

            if (tournament == null)
            {
                return NotFound();
            }

            return Ok(tournament);
        }

        // PUT: api/Tournaments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTournament([FromRoute] int id, [FromBody] Tournament tournament)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tournament.Id)
            {
                return BadRequest();
            }

            _context.Entry(tournament).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TournamentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tournaments
        [HttpPost]
        public async Task<IActionResult> PostTournament([FromBody] Tournament tournament)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            for(var i = 0; i < tournament.Competitors.Count; i++)
            {
                if (string.IsNullOrEmpty(tournament.Competitors[i].Name))
                    tournament.Competitors[i].Name = "Competitor " + (i + 1);
            }

            var seededCompetitors = tournament.Competitors.Where(s => s.Seed != 0).OrderBy(s => s.Seed).ToList();
            var unseededCompetitors = tournament.Competitors.Where(s => s.Seed == 0).ToList();

            // adjust seeded competitors for gaps/ties
            var seed = 1;
            for (var i = 0; i < seededCompetitors.Count; i++)
            {
                seededCompetitors[i].Seed = seed++;
            }
            // add and seed unseeded randomly
            Random rnd = new Random();
            while (unseededCompetitors.Count > 0)
            {
                int next = rnd.Next(0, unseededCompetitors.Count);
                var competitor = unseededCompetitors[next];
                competitor.Seed = seed++;

                seededCompetitors.Add(competitor);
                unseededCompetitors.Remove(competitor);
            }

            tournament.Competitors = seededCompetitors;

            var toNextNearest = NearestPowerOf2(tournament.Competitors.Count);

            var matchSeeds = new List<List<int>>();
            for (var i = 0; i < toNextNearest; i++)
            {
                var list = new List<int>();
                list.Add(i + 1);
                matchSeeds.Add(list);
            }
            while (matchSeeds.Count > 1)
            {
                List<List<int>> tempList = new List<List<int>>();
                for (var i = 0; i < matchSeeds.Count / 2; i++)
                {
                    tempList.Add(matchSeeds[i].Concat(matchSeeds[matchSeeds.Count - 1 - i]).ToList());
                }

                matchSeeds = tempList;
            }

            var winnersRounds = new List<Round>();
            var losersRounds = new List<Round>();

            var winnersMatches = new List<Match>();
            var losersMatches = new List<Match>();
            var matchNumber = 1;
            var roundNumber = 1;

            // populate first winners round
            for (var i = 0; i < matchSeeds[0].Count; i += 2)
            {
                var match = new Match();
                if (seededCompetitors.Count >= matchSeeds[0][i])
                {
                    match.CompetitorOne = seededCompetitors[matchSeeds[0][i] - 1];
                }
                if (seededCompetitors.Count >= matchSeeds[0][i + 1])
                {
                    match.CompetitorTwo = seededCompetitors[matchSeeds[0][i + 1] - 1];
                }

                match.MatchNumber = matchNumber++;
                winnersMatches.Add(match);
            }

            winnersRounds.Add(new Round {
                RoundType = RoundType.Winners,
                Matches = winnersMatches,
                RoundNumber = roundNumber++,
                Name = GenerateRoundName(winnersMatches.Count * 2, RoundType.Winners)
            });

            // populate first losers round if double elim
            if (tournament.IsDoubleElim)
            {
                for (var i = 0; i < winnersMatches.Count; i += 2)
                {
                    var match = new Match
                    {
                        //PrevMatchOne = winnersMatches[i],
                        //PrevMatchTwo = winnersMatches[i + 1],
                        MatchNumber = matchNumber++
                    };

                    winnersMatches[i].NextLosersMatch = match;
                    winnersMatches[i].NextLosersMatchSlot = Slot.One;
                    winnersMatches[i + 1].NextLosersMatch = match;
                    winnersMatches[i + 1].NextLosersMatchSlot = Slot.Two;

                    losersMatches.Add(match);
                }

                losersRounds.Add(new Round {
                    RoundType = RoundType.Losers,
                    Matches = losersMatches,
                    RoundNumber = roundNumber++,
                    Name = GenerateRoundName(losersMatches.Count * 2, RoundType.Losers, roundNumber - winnersRounds.Count - 1)
            });
            }

            // build bracket up to grand finals
            while (winnersRounds[winnersRounds.Count - 1].Matches.Count > 1)
            {
                var wRound = new Round {
                    RoundType = RoundType.Winners,
                    Matches = new List<Match>(),
                    RoundNumber = roundNumber++
                };

                for (int i = 0; i < winnersRounds[winnersRounds.Count - 1].Matches.Count; i += 2)
                {
                    var prevRoundMatches = winnersRounds[winnersRounds.Count - 1].Matches;

                    var match = new Match
                    {
                        //PrevMatchOne = prevRoundMatches[i],
                        //PrevMatchTwo = prevRoundMatches[i + 1],
                        MatchNumber = matchNumber++
                    };

                    prevRoundMatches[i].NextWinnersMatch = match;
                    prevRoundMatches[i].NextWinnersMatchSlot = Slot.One;
                    prevRoundMatches[i + 1].NextWinnersMatch = match;
                    prevRoundMatches[i + 1].NextWinnersMatchSlot = Slot.Two;

                    wRound.Matches.Add(match);
                }

                wRound.Name = GenerateRoundName(wRound.Matches.Count * 2, (RoundType)wRound.RoundType);
                winnersRounds.Add(wRound);

                if (tournament.IsDoubleElim)
                {
                    var lRound = new Round {
                        RoundType = RoundType.Losers,
                        Matches = new List<Match>(),
                        RoundNumber = roundNumber++
                    };

                    // TODO: alternate order of winners droping down each round
                    var prevRoundMatches = losersRounds[losersRounds.Count - 1].Matches;
                    for (var i = 0; i < wRound.Matches.Count; i++)
                    {
                        var match = new Match
                        {
                            //PrevMatchOne = wRound.Matches[i],
                            //PrevMatchTwo = prevRoundMatches[i],
                            MatchNumber = matchNumber++
                        };

                        prevRoundMatches[i].NextWinnersMatch = match;
                        prevRoundMatches[i].NextWinnersMatchSlot = Slot.Two;
                        wRound.Matches[i].NextLosersMatch = match;
                        wRound.Matches[i].NextLosersMatchSlot = Slot.One;

                        lRound.Matches.Add(match);
                    }

                    lRound.Name = GenerateRoundName(lRound.Matches.Count * 2, (RoundType)lRound.RoundType, roundNumber - winnersRounds.Count - 1);
                    losersRounds.Add(lRound);

                    // need to run extra set of losers matches
                    if (lRound.Matches.Count >= losersRounds[losersRounds.Count - 1].Matches.Count && lRound.Matches.Count > 1)
                    {
                        var prevLosersMatches = lRound.Matches;

                        lRound = new Round {
                            RoundType = RoundType.Losers,
                            Matches = new List<Match>(),
                            RoundNumber = roundNumber++
                        };

                        for (var i = 0; i < prevLosersMatches.Count; i += 2)
                        {
                            var match = new Match
                            {
                                //PrevMatchOne = prevLosersMatches[i],
                                //PrevMatchTwo = prevLosersMatches[i + 1],
                                MatchNumber = matchNumber++
                            };

                            prevLosersMatches[i].NextWinnersMatch = match;
                            prevLosersMatches[i].NextWinnersMatchSlot = Slot.One;
                            prevLosersMatches[i + 1].NextWinnersMatch = match;
                            prevLosersMatches[i + 1].NextWinnersMatchSlot = Slot.Two;

                            lRound.Matches.Add(match);
                        }

                        lRound.Name = GenerateRoundName(lRound.Matches.Count * 2, (RoundType)lRound.RoundType, roundNumber - winnersRounds.Count - 1);
                        losersRounds.Add(lRound);
                    }
                }
            }
            
            if(tournament.IsDoubleElim)
            {
                var grandFinals = new Round
                {
                    RoundType = RoundType.GrandFinals,
                    Matches = new List<Match>(),
                    RoundNumber = roundNumber++
                };

                var match = new Match { MatchNumber = matchNumber++ };

                var prevWinnersMatch = winnersRounds[winnersRounds.Count - 1].Matches[winnersRounds[winnersRounds.Count - 1].Matches.Count - 1];
                var prevLosersMatch = losersRounds[losersRounds.Count -1].Matches[losersRounds[losersRounds.Count - 1].Matches.Count - 1];

                prevWinnersMatch.NextWinnersMatch = match;
                prevWinnersMatch.NextWinnersMatchSlot = Slot.One;

                prevLosersMatch.NextWinnersMatch = match;
                prevLosersMatch.NextWinnersMatchSlot = Slot.Two;

                grandFinals.Matches.Add(match);
                grandFinals.Name = GenerateRoundName(grandFinals.Matches.Count * 2, (RoundType)grandFinals.RoundType);
                winnersRounds.Add(grandFinals);
            }

            tournament.Rounds = winnersRounds;

            if (tournament.IsDoubleElim)
            {
                tournament.Rounds = tournament.Rounds.Concat(losersRounds).ToList();
            }

            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTournament", new { id = tournament.Id }, tournament);
        }

        // NOTE: only works up to 32bit int
        private int NearestPowerOf2(int x)
        {
            if (x < 0) { return 0; }
            --x;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        private string GenerateRoundName(int numCompetitors, RoundType roundType, int losersRoundNum = -1)
        {
            var roundName = "";

            switch(roundType)
            {
                case RoundType.Winners:
                    switch (numCompetitors)
                    {
                        case 2:
                            roundName = "Winners Finals";
                            break;
                        case 4:
                            roundName = "Winners Semifinals";
                            break;
                        default:
                            roundName = "Round of " + numCompetitors;
                            break;
                    }
                    break;
                case RoundType.Losers:
                    roundName = "Losers Round " + losersRoundNum;
                    break;
                case RoundType.GrandFinals:
                    roundName = "Grand Finals";
                    break;
                default:
                    break;
            }

            return roundName;
        }


        // DELETE: api/Tournaments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTournament([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }

            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();

            return Ok(tournament);
        }

        private bool TournamentExists(int id)
        {
            return _context.Tournaments.Any(e => e.Id == id);
        }
    }
}