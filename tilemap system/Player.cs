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
    internal class Player
    {
        static Texture2D _texture;
        static int size = 12;
        Vector2 _position = new();
        Vector2 _speed = new();

        public Player(Vector2 position)
        {
            _position = position;
        }
        public Player(Point position)
        {
            _position = new(position.X, position.Y);
        }

        public static void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }

        public void Draw(SpriteBatch _spritebatch, Vector2 screenSize)
        {
            _spritebatch.Draw(_texture, new Rectangle(((int)screenSize.X - size) / 2, ((int)screenSize.Y - size) / 2, size, size), Color.Red);
        }

        public void update(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.A))
            {
                _speed.X -= 1;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                _speed.Y += 1;
            }
            if (keyboardState.IsKeyDown(Keys.W))
            {
                _speed.Y -= 1;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                _speed.X += 1;
            }
            _position += General.Normalize(_speed, 5);
            //_position.X += _speed.X;
            //_position.Y += _speed.Y;
            _speed = new();
        }
        public int GetSize()
        {
            return size;
        }
        public Vector2 GetPosition()
        {
            return _position;
        }
        public Rectangle GetRectangle()
        {
            return new Rectangle((int)_position.X - size/2, (int)_position.Y - size / 2, size, size);
        }
    
    }
}
