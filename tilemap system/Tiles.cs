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
        static int _size = 20;
        static int maxHealth = 100;
        Rectangle _collideRectangle;
        int health = maxHealth;

        public Tiles(Point point)
        {
            //_collideRectangle = new Rectangle(_size * point.X, _size * point.Y, _size, _size);
            _collideRectangle = new (point.X * _size, point.Y * _size, _size, _size);
        }

        public static void setTexture(Texture2D texture)
        {
            _texture = texture;
        }
        public void draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            int color = (int)((float)health / maxHealth * 255);
            spriteBatch.Draw(_texture,
                new Rectangle(_collideRectangle.X + (int)offset.X, _collideRectangle.Y + (int)offset.Y, _size, _size),
                null,
                new Color(color, color, color)
                );
        }
        public static Point getTileIndex(Vector2 position, Tiles[][] _Tiles)
        {
            Vector2 tileIndex = position / _size;
            return new Point((int)tileIndex.X, (int)tileIndex.Y);
        }

        public static Tiles? getTile(Vector2 position, Tiles[][] _Tiles)
        {
            Vector2 tileIndex = position / _size;
            if (tileIndex.X > _Tiles.Length || tileIndex.Y > _Tiles[0].Length || tileIndex.X < 0 || tileIndex.Y < 0)
            {
                return new Tiles(new(0, 0));
            }
            return _Tiles[(int)tileIndex.X][(int)tileIndex.Y];
        }
        public static List<Tiles> CollidingTiles(Rectangle rectangle, Point TileArray, Tiles[][] _Tiles)
        {
            List<Tiles?> tiles = new List<Tiles?>();

            Point point1 = getTileIndex(new Vector2(rectangle.Left, rectangle.Top), _Tiles);
            Point point2 = getTileIndex(new Vector2(rectangle.Right, rectangle.Bottom), _Tiles);
                    

            for (int x = Math.Max(point1.X, 0); x < Math.Min(point2.X + 1, TileArray.X); x++)
            {
                for (int y = Math.Max(point1.Y, 0); y < Math.Min(point2.Y + 1, TileArray.Y); y++)
                {
                    tiles.Add(_Tiles[x][y]);
                }
            }
            return tiles;
        }
        public static List<Tiles?> getLoaded(Vector2 focusPoint, Point range, Point TileArray, Tiles[][] _Tiles)
        {
            Point CameraTileIndex = Tiles.getTileIndex(focusPoint, _Tiles);
            List<Tiles?> tiles = new List<Tiles?>();
            range = new(
                (int)Math.Round((float)range.X / _size) + 1,
                (int)Math.Round((float)range.Y / _size) + 1);

            for (int x = Math.Max(CameraTileIndex.X - range.X + 1, 0); x < Math.Min(CameraTileIndex.X + range.X, TileArray.Y); x++)
            {
                for (int y = Math.Max(CameraTileIndex.Y - range.Y + 1, 0); y < Math.Min(CameraTileIndex.Y + range.Y, TileArray.X); y++)
                {
                    tiles.Add(_Tiles[x][y]);
                }
            }

            return tiles;
        }
        public Rectangle GetRectangle()
        {
            return _collideRectangle;
        }
        public static int getSize()
        {
            return _size;
        }
        public void Update()
        {
            if (health > 0)
            {
                health = Math.Min(health + 1, maxHealth);
            }
        }
        public void MineTile(int damage)
        {
            health = Math.Max(health - damage, 0);
        }

    }
}
