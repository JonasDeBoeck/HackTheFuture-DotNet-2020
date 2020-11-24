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
        private int index=0;

        public MyAdventure() { 
        }
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
            var pathfinder = new PathFinder(request.Map);
            var start_location = PathFinder.GetAllTileTypes(request.Map, TileType.Start)[0];
            // find loot
            var Treasures = pathfinder.GetAllPaths(start_location, TileType.TreasureChest);
            var Enemies = PathFinder.GetAllTileTypes(request.Map, TileType.Enemy);
            var EnemiesOnPath = CalculateEnemies();


            // calculate finish
            var path = pathfinder.A_star(start_location, PathFinder.GetAllTileTypes(request.Map,TileType.Finish)[0]);
                    
            return SmartPath();

            Task<Turn> PlayToEnd()
            {
                return Task.FromResult(request.PossibleActions.Contains(TurnAction.WalkSouth) ? new Turn(TurnAction.WalkSouth) : new Turn(request.PossibleActions[_random.Next(request.PossibleActions.Length)]));
            }

            Task<Turn> SmartPath()
            {
                var start = path[index];
                if (index+1 < path.Count)
                {
                    var location = path[++index];
                    double deltaX = start.X - location.X;
                    double deltaY = start.Y - location.Y;
                    if (request.Map.Tiles[start.X,start.Y].TileType == TileType.Enemy)
                    {
                        
                        if (request.PossibleActions.Contains(TurnAction.Attack))
                        {
                            index--;
                            return Task.FromResult(new Turn(TurnAction.Attack));
                        }
                        else if (request.PossibleActions.Contains(TurnAction.Loot))
                        {
                            index--;
                            return Task.FromResult(new Turn(TurnAction.Loot));
                        }
                        else
                        {
                            return Task.FromResult(new Turn(TurnAction.WalkEast));
                        }
                    }
                    else if (request.Map.Tiles[start.X,start.Y].TileType == TileType.TreasureChest)
                    {
                        
                        if (request.PossibleActions.Contains(TurnAction.Loot))
                        {
                            index--;
                            return Task.FromResult(new Turn(TurnAction.Loot));
                        }
                    }

                    if (deltaX == -1 && deltaY == 0)
                    {
                        return Task.FromResult(new Turn(TurnAction.WalkEast));
                    }
                    else if (deltaX == 1 && deltaY == 0)
                    {
                        return Task.FromResult(new Turn(TurnAction.WalkWest));
                    }
                    else if (deltaX == 0 && deltaY == -1)
                    {
                        return Task.FromResult(new Turn(TurnAction.WalkSouth));
                    }
                    else
                    {
                        return Task.FromResult(new Turn(TurnAction.WalkNorth));
                    }
                }
                return Task.FromResult(new Turn(request.PossibleActions[_random.Next(request.PossibleActions.Length)]));
            }

            List<int> CalculateEnemies()
            {
                List<int> matrix = new List<int>();
                foreach (var treasure_path in Treasures)
                {
                    int EnemyCount = 0;
                    foreach (var EnemyLocation in Enemies)
                    {
                        if (treasure_path.Contains(EnemyLocation))
                        {
                            EnemyCount++;                            
                        }
                    }
                    matrix.Add(EnemyCount);
                }
                return matrix;
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