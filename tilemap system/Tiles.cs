using System;
using System.Collections.Generic;
using System.Linq;
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
        static int _size = 50;
        Rectangle _collideRectangle;

        public Tiles(Rectangle point)
        {
            //_collideRectangle = new Rectangle(_size * point.X, _size * point.Y, _size, _size);
            _collideRectangle = point;
        }

        public static void setTexture(Texture2D texture)
        {
            _texture = texture;
        }

        public void draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            spriteBatch.Draw(_texture,
                new Rectangle (_collideRectangle.X + (int)offset.Y, _collideRectangle.Y + (int)offset.Y, _size, _size),
                null,
                Color.White
                );
        }
        public static Point getTileIndex(Vector2 position, Tiles[][] _Tiles)
        {
            Vector2 tileIndex = position / _size;
            return new Point((int)tileIndex.X, (int)tileIndex.Y);
        }

        public static object getTile(Vector2 position, Tiles[][] _Tiles)
        {
            Vector2 tileIndex = position / _size;
            return _Tiles[(int)tileIndex.X][(int)tileIndex.Y];
        }
    }
}
