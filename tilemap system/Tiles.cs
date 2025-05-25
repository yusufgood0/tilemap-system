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
        static int zSize = 1;
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
                    _collideCube.Z
                    );
        }
        public static Point getTileIndex(Vector3 position, Tiles[][] _Tiles)
        {
            int x = (int)(position.X / xSize);
            int y = (int)(position.Y / ySize);
            int z = (int)(position.Z / zSize);
            return new Point(x, y);
        }

        public static Tiles? getTile(Vector3 position, Tiles[][] _Tiles)
        {
            int x = (int)(position.X / xSize);
            int y = (int)(position.Y / ySize);
            int z = (int)(position.Z / zSize);
            if (y > _Tiles.Length || y > _Tiles[0].Length || y < 0 || y < 0)
            {
                return new Tiles(new(0, 0, 0));
            }
            return _Tiles[x][y];
        }
        public static List<Tiles> CollidingTiles(Rectangle rectangle, Point TileArray, Tiles[][] _Tiles)
        {
            List<Tiles?> tiles = new List<Tiles?>();

            Point point1 = getTileIndex(new Vector3(rectangle.Left, rectangle.Top, 0), _Tiles);
            Point point2 = getTileIndex(new Vector3(rectangle.Right, rectangle.Bottom, 0), _Tiles);


            for (int x = point1.X; x < point2.X + 1; x++)
            {
                for (int y = point1.Y; y < point2.Y + 1; y++)
                {
                    if (TileArray.X > x && x > 0 && TileArray.Y > y && y > 0)
                    {
                        tiles.Add(_Tiles[x][y]);
                    }
                }
            }

            return tiles;
            
        }
        public static List<Tiles?> getLoaded(Vector2 focusPoint, Point range, Point TileArray, Tiles[][] _Tiles)
        {
            Point CameraTileIndex = Tiles.getTileIndex(new(focusPoint, 0), _Tiles);
            List<Tiles?> tiles = new List<Tiles?>();
            range = new(
                (int)Math.Round((float)range.X / xSize) + 1,
                (int)Math.Round((float)range.Y / ySize) + 1);

            for (int x = Math.Max(CameraTileIndex.X - range.X + 1, 0); x < Math.Min(CameraTileIndex.X + range.X, TileArray.Y); x++)
            {
                for (int y = Math.Max(CameraTileIndex.Y - range.Y + 1, 0); y < Math.Min(CameraTileIndex.Y + range.Y, TileArray.X); y++)
                {
                    tiles.Add(_Tiles[x][y]);
                }
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
