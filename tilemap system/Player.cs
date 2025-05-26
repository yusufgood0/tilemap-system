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
        static int size = 30;
        Vector3 _position = new();
        Vector3 _speed = new();

        public Player(Vector3 position)
        {
            _position = position;
        }
        public static void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }
        public void Draw(SpriteBatch _spritebatch, Vector2 OFFSET)
        {
            _spritebatch.Draw(_texture, 
                new Rectangle(Rectangle.X + (int)OFFSET.X, Rectangle.Y + (int)OFFSET.Y, Rectangle.Width, Rectangle.Height), 
                Color.Red);
        }
        public void CollisionX(Cube Cube)
        {
            if (_speed.X > 0)
            {
                _position.X = Cube.X - size - .1f;
                _speed.X = 0;
            }
            else if (_speed.X < 0)
            {
                _position.X = Cube.X_OP + .1f;
                _speed.X = 0;
            }
        }
        public void CollisionY(Cube Cube)
        {
            if (_speed.Y > 0)
            {
                _speed.Y = 0;
                _position.Y = Cube.Y - size - .1f;
            }
            else if (_speed.Y < 0)
            {
                _position.Y = Cube.Y_OP + .1f;
                _speed.Y = 0;
            }
        }
        public void CollisionZ(Cube Cube)
        {
            if (_speed.Z > 0)
            {
                _speed.Z = 0;
                _position.Z = Cube.Z - size - .1f;
            }
            else if (_speed.Z < 0)
            {
                _position.Z = Cube.Z_OP + .1f;
                _speed.Z = 0;
            }
        }
        public void update(KeyboardState keyboardState, KeyboardState PreviouskeyboardState)
        {
            _speed.Y += .4f;

            Vector2 normalizedSpeed = new();
            if (keyboardState.IsKeyDown(Keys.W)) { normalizedSpeed.Y -= 1; }
            if (keyboardState.IsKeyDown(Keys.S)) { normalizedSpeed.Y += 1; }
            if (keyboardState.IsKeyDown(Keys.A)) { normalizedSpeed.X -= 1; }
            if (keyboardState.IsKeyDown(Keys.D)) { normalizedSpeed.X += 1; }
            normalizedSpeed = General.Normalize(normalizedSpeed, .1f);

            _speed.X += normalizedSpeed.X;
            _speed.Z += normalizedSpeed.Y;


            _speed.X *= .75f;
            _speed.Y *= .99f;
            _speed.Z *= .75f;
        }
        public void jump()
        {
            _speed.Y -= 10;
        }
        public void move(Vector3 vector)
        {
            _position += vector;
        }

        public Vector2 XY { get => new(_position.X, _position.Y); set; }
        public Vector3 Speed { get => _speed; set; }
        public Rectangle Rectangle { get => new((int)_position.X, (int)_position.Y, size, size); set; }
        public Rectangle Cube { get => new((int)_position.X, (int)_position.Y, size, size); set; } // working here


    }
}
