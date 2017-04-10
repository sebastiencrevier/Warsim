using System;
using System.Collections.Generic;

using Warsim.Core.Game.Entities;

namespace Warsim.Client
{
    public static class MapTests
    {
        public static IList<Node> GetNodes()
        {
            var nodes = new List<Node>
            {
                new Node
                {
                    Type = "mur",
                    Position = new Vector(-114.3, 11, -25),
                    Rotation = new Vector(0, -77.8, 0),
                    Scale = new Vector(7.5, 7.5, 68.4)
                },
                new Node
                {
                    Type = "mur",
                    Position = new Vector(-84.6, 11, -50.2),
                    Rotation = new Vector(0, -93.6, 0),
                    Scale = new Vector(7.5, 7.5, 65.8)
                },
                new Node
                {
                    Type = "lignes",
                    Position = new Vector(-23.8, 0, -15.4),
                    Rotation = new Vector(0, 0, 0),
                    Scale = new Vector(1, 1, 1),
                    Children = new List<Node>
                    {
                        new Node
                        {
                            Type = "ligne",
                            Position = new Vector(-18.7, 0, 50.1),
                            Rotation = new Vector(0, 337.8, 0),
                            Scale = new Vector(8.333, 8.333, 41.1)
                        },
                        new Node
                        {
                            Type = "ligne",
                            Position = new Vector(-4.7, 0, 12.5),
                            Rotation = new Vector(0, 78.3, 0),
                            Scale = new Vector(8.333, 8.333, 43.1)
                        },
                        new Node
                        {
                            Type = "ligne",
                            Position = new Vector(-46.3, 0, 3.8),
                            Rotation = new Vector(0, 284.6, 0),
                            Scale = new Vector(8.333, 8.333, 92.4)
                        },
                        new Node
                        {
                            Type = "ligne",
                            Position = new Vector(41.3, 0, -20.5),
                            Rotation = new Vector(0, 14.7, 0),
                            Scale = new Vector(8.333, 8.333, 22.3)
                        },
                    }
                },
                new Node
                {
                    Type = "poteau",
                    Position = new Vector(-84.6, 0, 30.4),
                    Rotation = new Vector(0, 0, 0),
                    Scale = new Vector(7.5, 7.5, 7.5)
                },
                new Node
                {
                    Type = "poteau",
                    Position = new Vector(87.9, 0, 41.8),
                    Rotation = new Vector(0, 0, 0),
                    Scale = new Vector(47.5, 7.5, 47.5)
                },
                new Node
                {
                    Type = "lignes",
                    Position = new Vector(69.3, 0, -7.6),
                    Rotation = new Vector(0, -52, 0),
                    Scale = new Vector(1, 1, 1),
                    Children = new List<Node>
                    {
                        new Node
                        {
                            Type = "ligne",
                            Position = new Vector(-18.7, 0, 50.1),
                            Rotation = new Vector(0, 337.8, 0),
                            Scale = new Vector(8.333, 8.333, 41.1)
                        },
                        new Node
                        {
                            Type = "ligne",
                            Position = new Vector(-4.7, 0, 12.5),
                            Rotation = new Vector(0, 78.3, 0),
                            Scale = new Vector(8.333, 8.333, 43.1)
                        },
                        new Node
                        {
                            Type = "ligne",
                            Position = new Vector(-46.3, 0, 3.8),
                            Rotation = new Vector(0, 284.6, 0),
                            Scale = new Vector(8.333, 8.333, 92.4)
                        },
                        new Node
                        {
                            Type = "ligne",
                            Position = new Vector(41.3, 0, -20.5),
                            Rotation = new Vector(0, 14.7, 0),
                            Scale = new Vector(8.333, 8.333, 22.3)
                        },
                    }
                },
            };

            return nodes;
        }
    }
}