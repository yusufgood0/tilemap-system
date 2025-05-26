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
    internal class Cube
    {
        //int x;
        //int xSize;
        //int y;
        //int ySize;
        //int z;
        //int zSize;
        public Cube(int x, int y, int z, int xSize, int ySize, int zSize)
        {
            this.X = x; this.XSize = xSize;
            this.Y = y; this.YSize = ySize;
            this.Z = z; this.ZSize = zSize;
        }
        public Cube(IntTriple triple, int xSize, int ySize, int zSize)
        {
            this.X = triple.X; this.XSize = xSize;
            this.Y = triple.Y; this.YSize = ySize;
            this.Z = triple.Z; this.ZSize = zSize;
        }
        public Cube(Vector3 vector, int xSize, int ySize, int zSize)
        {
            this.X = (int)vector.X; this.XSize = xSize;
            this.Y = (int)vector.Y; this.YSize = ySize;
            this.Z = (int)vector.Z; this.ZSize = zSize;
        }

        public int X { get; set; }
        public int X_OP { get => X + XSize; }
        public int Y { get; set; }
        public int Y_OP { get => Y + YSize; }
        public int Z { get; set; }
        public int Z_OP { get => Z + ZSize; }

        public int XSize { get; set; }
        public int YSize { get; set; }
        public int ZSize { get; set; }
        public Rectangle Rectangle { get => new(X, Y, XSize, YSize); set; }
        public IntTriple Position { get => new(X, Y, Z); set; }
    }
}
