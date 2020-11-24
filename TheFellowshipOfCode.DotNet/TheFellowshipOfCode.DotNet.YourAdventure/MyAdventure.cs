using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HTF2020.Contracts;
using HTF2020.Contracts.Enums;
using HTF2020.Contracts.Models;
using HTF2020.Contracts.Models.Adventurers;
using HTF2020.Contracts.Models.Party;
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
                if (_random.Next(0, 10) > 5)
                {
                    party.Members.Add(new Wizard()
                    {
                        Id = i,
                        Name = $"Wizard {i + 1}",
                        Constitution = 11, 
                        Strength = 9,
                        Intelligence = 14
                    });
                }
                else
                {
                    party.Members.Add(new Fighter()
                    {
                        Id = i,
                        Name = $"Fighter {i + 1}",
                        Constitution = 11,
                        Strength = 14,
                        Intelligence = 9
                    });
                }
            }

            return Task.FromResult(party);
        }

        public Task<Turn> PlayTurn(PlayTurnRequest request)
        {
            return convertToActions();

            Task<Turn> PlayToEnd()
            {
                var start = PathFinder.GetAllTileTypes(request.Map, TileType.Start)[0];
                var finish = PathFinder.GetAllTileTypes(request.Map, TileType.Finish)[0];
                PathFinder path = new PathFinder(request.Map);
                var distance = path.CalculateDistance(start, finish);
                Console.WriteLine(distance);
                var p = path.A_star(start,finish);
                return Task.FromResult(request.PossibleActions.Contains(TurnAction.WalkSouth) ? new Turn(TurnAction.WalkSouth) : new Turn(request.PossibleActions[_random.Next(request.PossibleActions.Length)]));
            }

            Task<Turn> convertToActions()
            {
                var start = PathFinder.GetAllTileTypes(request.Map, TileType.Start)[0];
                var pathfinder = new PathFinder(request.Map);
                var path = pathfinder.A_star(start, PathFinder.GetAllTileTypes(request.Map, TileType.Finish)[0]);
                foreach (var location in path)
                {
                    double deltaX = start.X - location.X;
                    double deltaY = start.Y - location.Y;
                    if (deltaX == -1 && deltaY == 0)
                    {
                        return Task.FromResult(new Turn(TurnAction.WalkWest));
                    }
                    if (deltaX == 1 && deltaY == 0)
                    {
                        return Task.FromResult(new Turn(TurnAction.WalkEast));
                    }
                    if (deltaX == 0 && deltaY == -1)
                    {
                        return Task.FromResult(new Turn(TurnAction.WalkSouth));
                    }
                    if (deltaX == 0 && deltaY == 1)
                    {
                        return Task.FromResult(new Turn(TurnAction.WalkNorth));
                    }
                    start = location;
                }
                return Task.FromResult(new Turn(request.PossibleActions[_random.Next(request.PossibleActions.Length)]));
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

            /*Task<Turn> play()
            {
                // locate all treasures
                // GetAllTileTypes(request.Map, TileType.TreasureChest);

                // find shortest path to first
            }*/
        }
    }
}