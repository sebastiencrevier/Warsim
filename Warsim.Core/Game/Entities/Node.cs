using System;
using System.Collections.Generic;

namespace Warsim.Core.Game.Entities
{
    public class Node
    {
        public Node()
        {
        }

        public Node(Node n)
        {
            this.Id = n.Id;
            this.Type = n.Type;
            this.Deleted = n.Deleted;
            this.Selected = n.Selected;
            this.LastUpdated = n.LastUpdated;
            this.Position = n.Position;
            this.Rotation = n.Rotation;
            this.Scale = n.Scale;
            this.LastUserId = n.LastUserId;
            this.SelectedColor = n.SelectedColor;
            this.Children = new List<Node>();

            if (n.Children != null)
            {
                foreach (var child in n.Children)
                {
                    this.Children.Add(new Node(child));
                }
            }
        }

        public string Id { get; set; }

        public string Type { get; set; }

        public string LastUserId { get; set; }

        public DateTime LastUpdated { get; set; }

        public bool Selected { get; set; }

        public bool Deleted { get; set; }

        public Vector SelectedColor { get; set; }

        public Vector Position { get; set; }

        public Vector Rotation { get; set; }

        public Vector Scale { get; set; }

        public IList<Node> Children { get; set; }
    }
}