using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using HTF2020.Contracts;
using HTF2020.Contracts.Enums;
using HTF2020.Contracts.Models;
using HTF2020.Contracts.Models.Adventurers;
using HTF2020.Contracts.Requests;

namespace TheFellowshipOfCode.DotNet.YourAdventure
{
    public class LocationWrapper
    {
        public Location Location { get; set; }
        public LocationWrapper Parent { get; set; }
        public double G_score { get; set; }
        public double H_score { get; set; }

        public LocationWrapper(Location location)
        {
            this.Location = location;
            G_score = 0;
            H_score = 0;
        }
    }

    class PathFinder
    {
        private Map map;
        public PathFinder(Map map)
        {
            this.map = map;
        }
        public IList<Location> A_star(Location Start, Location Finish)
        {
            IList<LocationWrapper> openlist = new List<LocationWrapper>();
            IList<LocationWrapper> closedList = new List<LocationWrapper>();
            var StartWrapper = new LocationWrapper(Start);
            StartWrapper.Parent = null;
            openlist.Add(StartWrapper);
            while (openlist.Count > 0)
            {
                var current = GetMin(openlist, Finish);
                if (current.Location.Y == 8)
                {
                    Console.WriteLine("idk");
                }
                openlist.Remove(current);
                if (current.Location.X == Finish.X && current.Location.Y == Finish.Y)
                {
                    return Backtrack(current);
                }
                var Neighbours = GetNeighbours(current.Location);
                closedList.Add(current);
                foreach (var location in Neighbours)
                {
                    if (!contains(closedList,location))
                    {
                        var new_G = current.G_score + 1;
                        if (openlist.Contains(location))
                        {
                            if (new_G < location.G_score)
                            {
                                location.Parent = current;
                                location.G_score = new_G;
                            }
                        }
                        else
                        {
                            if (location.Parent == null)
                            {
                                location.Parent = current;
                            }
                            location.G_score = new_G;
                            openlist.Add(location);
                        }
                        location.H_score = CalculateDistance(location.Location, Finish);
                    }
                }

            
            }
            return null;
        }

        private bool contains(IList<LocationWrapper> closedList, LocationWrapper location)
        {
            foreach(var l in closedList)
            {
                if (l.Location.X == location.Location.X && l.Location.Y == location.Location.Y)
                {
                    return true;
                }
            }
            return false;
        }

        private IList<Location> Backtrack(LocationWrapper current)
        {
            IList<Location> path = new List<Location>();
            while (current.Parent != null)
            {
                path.Add(current.Parent.Location);
                current = current.Parent;
            }
            return path.Reverse().ToList();
        }

        public double CalculateDistance(Location start, Location finish)
        {
            double DeltaX = Math.Abs(finish.X - start.X);
            double DeltaY = Math.Abs(finish.Y - start.Y);
            return Math.Sqrt(Math.Pow(DeltaX, 2) + Math.Pow(DeltaY, 2));
        }

        private IList<LocationWrapper> GetNeighbours(Location current)
        {
            IList<LocationWrapper> neighbours = new List<LocationWrapper> ();
            // north of current
            LocationWrapper north = new LocationWrapper(new Location(current.X - 1, current.Y));
            LocationWrapper south = new LocationWrapper(new Location (current.X + 1, current.Y));
            LocationWrapper west = new LocationWrapper(new Location (current.X, current.Y - 1));
            LocationWrapper east = new LocationWrapper(new Location (current.X, current.Y + 1));
            if (north.Location.X > 0)
                neighbours.Add(north);
            if (south.Location.X < this.map.Tiles.GetLength(0))
                neighbours.Add(south);
            if (east.Location.X < this.map.Tiles.GetLength(1))
                neighbours.Add(east);
            if (west.Location.X > 0 )
                neighbours.Add(west);
            return checkNeighbours(neighbours);
        }

        private IList<LocationWrapper> checkNeighbours(IList<LocationWrapper> locations)
        {
            IList<LocationWrapper> realNeighbours = new List<LocationWrapper>();
            foreach(var location in locations)
            {
                var tile = this.map.Tiles[location.Location.X, location.Location.Y];
                if (tile.TileType == TileType.Empty || tile.TileType == TileType.Finish)
                {
                    if (tile.TerrainType == TerrainType.Grass)
                    {
                        realNeighbours.Add(location);
                    }
                }
            }
            return realNeighbours;
        }

        public LocationWrapper GetMin(IList<LocationWrapper> list, Location Finish)
        {
            var minLocation = list[0];
            foreach(var location in list)
            {
                if (minLocation.H_score + minLocation.G_score > location.H_score + location.G_score)
                {
                    minLocation = location;
                }
                /*if (CalculateDistance(location.Location,Finish)< CalculateDistance(minLocation.Location, Finish))
                {
                    minLocation = location;
                }*/
            }
            return minLocation;
        }

        public static IList<Location> GetAllTileTypes(Map map, TileType type)
        {
            IList<Location> locations = new List<Location>();
            for (int i = 0; i < map.Tiles.GetLength(0); i++)
            {
                for (int j = 0; j < map.Tiles.GetLength(1); j++)
                {
                    if (map.Tiles[i, j].TileType == type)
                    {
                        locations.Add(new Location(i, j));
                    }
                }
            }
            return locations;
        }
    }
}
