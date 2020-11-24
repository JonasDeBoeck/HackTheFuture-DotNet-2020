using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HTF2020.Contracts;
using HTF2020.Contracts.Enums;
using HTF2020.Contracts.Models;
using HTF2020.Contracts.Models.Adventurers;
using HTF2020.Contracts.Requests;

namespace TheFellowshipOfCode.DotNet.YourAdventure
{
    public class MyAdventure : IAdventure
    {
        private readonly Random _random = new Random();

        public Task<Party> CreateParty(CreatePartyRequest request)
        {
            var party = new Party
            {
                Name = "My Party",
                Members = new List<PartyMember>()
            };

            for (var i = 0; i < request.MembersCount; i++)
            {
               party.Members.Add(new Fighter()
                {
                    Id = i,
                    Name = $"Member {i + 1}",
                    Constitution = 11,
                    Strength = 12,
                    Intelligence = 11
                });
            }

            return Task.FromResult(party);
        }

        public Task<Turn> PlayTurn(PlayTurnRequest request)
        {
            // return Strategic();
            // return PlayToEnd();
            return SmartPath();

            Task<Turn> PlayToEnd()
            {
                return Task.FromResult(request.PossibleActions.Contains(TurnAction.WalkSouth) ? new Turn(TurnAction.WalkSouth) : new Turn(request.PossibleActions[_random.Next(request.PossibleActions.Length)]));
            }

            Task<Turn> SmartPath()
            {
                var start = PathFinder.GetAllTileTypes(request.Map, TileType.Start)[0];
                var pathfinder = new PathFinder(request.Map);
                var path = pathfinder.A_star(start, TileType.Enemy);
                IList<Turn> actions = new List<Turn>();
                foreach (var location in path)
                {
                    double deltaX = start.X - location.X;
                    double deltaY = start.Y - location.Y;
                    if (deltaX == -1 && deltaY == 0)
                    {
                        Task.FromResult(new Turn(TurnAction.WalkWest));
                        //actions.Add(new Turn(TurnAction.WalkWest));
                    }
                    if (deltaX == 1 && deltaY == 0)
                    {
                        Task.FromResult(new Turn(TurnAction.WalkEast));
                        /* actions.Add(new Turn(TurnAction.WalkEast));*/
                    }
                    if (deltaX == 0 && deltaY == -1)
                    {
                        Task.FromResult(new Turn(TurnAction.WalkSouth));
                        /*actions.Add(new Turn(TurnAction.WalkSouth));*/
                    }
                    if (deltaX == 0 && deltaY == 1)
                    {
                        Task.FromResult(new Turn(TurnAction.WalkNorth));
                        /*actions.Add(new Turn(TurnAction.WalkNorth));*/
                    }
                    start = location;
                }
            }

            void out_test(out bool idk)
            {
                idk = false
            }

            Task<Turn> Strategic()
            {
                const double goingEastBias = 0.35;
                const double goingSouthBias = 0.25;
                if (request.PossibleActions.Contains(TurnAction.Loot))
                {
                    return Task.FromResult(new Turn(TurnAction.Loot));
                }

                if (request.PossibleActions.Contains(TurnAction.Attack))
                {
                    return Task.FromResult(new Turn(TurnAction.Attack));
                }

                if (request.PossibleActions.Contains(TurnAction.WalkEast) && _random.NextDouble() > (1 - goingEastBias))
                {
                    return Task.FromResult(new Turn(TurnAction.WalkEast));
                }

                if (request.PossibleActions.Contains(TurnAction.WalkSouth) && _random.NextDouble() > (1 - goingSouthBias))
                {
                    return Task.FromResult(new Turn(TurnAction.WalkSouth));
                }

                return Task.FromResult(new Turn(request.PossibleActions[_random.Next(request.PossibleActions.Length)]));
            }
        }
    }
}