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
        float _x;
        int _xSize;
        float _y;
        int _ySize;
        float _z;
        int _zSize;
        public Cube(int x, int y, int z, int xSize, int ySize, int zSize)
        {
            _x = x; _xSize = xSize;
            _y = y; _ySize = ySize;
            _z = z; _zSize = zSize;
        }

        public float X { get => _x; set; }
        public float X_OP { get => _x + _xSize; set; }
        public float Y { get => _y; set; }
        public float Y_OP { get => _y + _ySize; set; }
        public float Z { get => _z; set; }
        public float Z_OP { get => _z + _zSize; set; }

        public int XSize { get => _xSize; set; }
        public int YSize { get => _ySize; set; }
        public int ZSize { get => _zSize; set; }
        public Rectangle Rectangle { get => new((int)_x, (int)_y, _xSize, _ySize); set; }
        public Vector3 Position { get => new(_x, _y, _z); set; }
    }
}
