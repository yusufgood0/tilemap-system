using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace tilemap_system
{
    internal class Tiles
    {
        static Texture2D _texture;
        static int maxHealth = 100;
        static int xSize = 40;
        static int ySize = 40;
        static int zSize = 40;
        Cube _collideCube = new(0, 0, 0, xSize, ySize, zSize);
        int _health = maxHealth;
        ID _type = ID.Full;
        enum ID
        {
            Empty,
            Full,
        }

        public Tiles(Vector3 point)
        {
            _collideCube.X = (int)point.X * xSize;
            _collideCube.Y = (int)point.Y * ySize;
            _collideCube.Z = (int)point.Z * zSize;
            //_collideCube = new((int)point.X * xSize, (int)point.Y * ySize, (int)point.Z * zSize, xSize, ySize, zSize);

        }

        public static void setTexture(Texture2D texture)
        {
            _texture = texture;
        }
        public void draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            Color color = Color.White;
            switch (_type)
            {
                case ID.Full:
                    int value = (int)((float)_health / maxHealth * 255);
                    color = new(value, value, 0);
                    break;
                case ID.Empty:
                    color = new(125, 125, 125);
                    break;
            }

            //int color = (int)((float)_collideCube.Z);
            spriteBatch.Draw(_texture,
                    new Rectangle(
                        (int)_collideCube.X + (int)offset.X,
                        (int)_collideCube.Y + (int)offset.Y,
                        _collideCube.XSize,
                        _collideCube.YSize),
                    null,
                    color,
                    0,
                    new(),
                    0,
                    //_collideCube.Z
                    0
                    );
        }
        public static IntTriple getTileIndex(Vector3 position, Tiles[][][] _Tiles)
        {
            int x = (int)(position.X / xSize);
            int y = (int)(position.Y / ySize);
            int z = (int)(position.Z / zSize);
            return new IntTriple(x, y, z);
        }

        public static Tiles? getTile(Vector3 position, Tiles[][][] _Tiles)
        {
            int x = (int)(position.X / xSize);
            int y = (int)(position.Y / ySize);
            int z = (int)(position.Z / zSize);
            if (x > _Tiles.Length || y > _Tiles[x].Length || z > _Tiles[x][y].Length || x < 0 || y < 0 || z < 0)
            {
                return new Tiles(new(0, 0, 0));
            }
            return _Tiles[x][y][z];
        }
        public static List<Tiles> CollidingTiles(Cube cube, IntTriple TileArray, Tiles[][][] _Tiles)
        {
            List<Tiles?> tiles = new List<Tiles?>();

            IntTriple point1 = getTileIndex(new Vector3(cube.X, cube.Y, cube.Z), _Tiles);
            IntTriple point2 = getTileIndex(new Vector3(cube.X_OP, cube.Y_OP, cube.Z_OP), _Tiles);


            for (int x = point1.X; x < point2.X + 1; x++)
            for (int y = point1.Y; y < point2.Y + 1; y++)
            for (int z = point1.Z; z < point2.Z + 1; z++)

                        if (TileArray.X > x && x > 0 && TileArray.Y > y && y > 0 && TileArray.Z > z && z > 0)
                        {
                            tiles.Add(_Tiles[x][y][z]);
                        }

            return tiles;

        }
        public static List<Tiles> getLoaded(Vector3 focusPoint, IntTriple range, IntTriple TileArray, Tiles[][][] _Tiles)
        {
            IntTriple CameraTileIndex = Tiles.getTileIndex(focusPoint, _Tiles);
            List<Tiles> tiles = new List<Tiles>();
            range = new(
                (int)Math.Round((float)range.X / xSize) + 1,
                (int)Math.Round((float)range.Y / ySize) + 1,
                (int)Math.Round((float)range.Z / ZSize) + 1
                );

            for (int x = Math.Max(CameraTileIndex.X - range.X, 0); x < Math.Min(CameraTileIndex.X + range.X, TileArray.X); x++)
            for (int y = Math.Max(CameraTileIndex.Y - range.Y, 0); y < Math.Min(CameraTileIndex.Y + range.Y, TileArray.Y); y++)
            for (int z = Math.Max(CameraTileIndex.Z - range.Z, 0); z < Math.Min(CameraTileIndex.Z + range.Z, TileArray.Z); z++)
                        if (TileArray.X > x && x > 0 && TileArray.Y > y && y > 0 && TileArray.Z > z && z > 0)
                        {
                            tiles.Add(_Tiles[x][y][z]);
                        }

            return tiles;
        }
        public void Update()
        {
            if (Isfull)
            {
                _health = Math.Min(_health + 1, maxHealth);
                _type = ID.Full;
            }
        }
        public void MineTile(int damage)
        {
            _health = _health - damage;
            if (_health < 0)
            {
                _health = 0;
                _type = ID.Empty;
            }
        }

        public Cube Cube { get => _collideCube; set; }
        static public int XSize { get => xSize; set; }
        static public int YSize { get => ySize; set; }
        static public int ZSize { get => zSize; set; }
        public bool Isfull { get => !(_type == ID.Empty); set; }
    }
}
