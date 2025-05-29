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
    internal struct Cube
    {
        public Cube(int x, int y, int z, int xSize, int ySize, int zSize)
        {
            X = x; XSize = xSize;
            Y = y; YSize = ySize;
            Z = z; ZSize = zSize;
        }
        public Cube(IntTriple triple, int xSize, int ySize, int zSize)
        {
            X = triple.X; XSize = xSize;
            Y = triple.Y; YSize = ySize;
            Z = triple.Z; ZSize = zSize;
        }
        public Cube(Vector3 vector, int xSize, int ySize, int zSize)
        {
            X = (int)vector.X; XSize = xSize;
            Y = (int)vector.Y; YSize = ySize;
            Z = (int)vector.Z; ZSize = zSize;
        }

        public int X { get; set; }
        public readonly int X_OP { get => X + XSize; }
        public int Y { get; set; }
        public readonly int Y_OP { get => Y + YSize; }
        public int Z { get; set; }
        public readonly int Z_OP { get => Z + ZSize; }

        public int XSize { get; set; }
        public int YSize { get; set; }
        public int ZSize { get; set; }
        public readonly Rectangle Rectangle { get => new (X, Y, XSize, YSize); }
        public readonly IntTriple Position { get => new(X, Y, Z); }
    }
}
