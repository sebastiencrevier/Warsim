using System;

namespace Warsim.Core.Game.Entities
{
    public class Vector
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public Vector(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}