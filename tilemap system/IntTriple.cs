using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Threading.Tasks;

namespace tilemap_system
{
    internal class IntTriple
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public IntTriple()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public IntTriple(int a, int b, int c)
        {
            X = a;
            Y = b;
            Z = c;
        }
        public IntTriple(Vector3 vector)
        {
            X = (int)vector.X;
            Y = (int)vector.Y;
            Z = (int)vector.Z;
        }

        public Vector2 XY { get => new(X, Y); set; }
        public Vector3 XYZ { get => new(X, Y, Z); set; }
    }
}
