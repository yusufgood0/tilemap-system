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
        public Vector2 _angle = Vector2.Zero;

        public Player(Vector3 position)
        {
            _position = position;
        }
        public Player(IntTriple position)
        {
            _position = new(position.X, position.Y, position.Z);
        }
        public static void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 OFFSET)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(_texture, 
                new Rectangle(Rectangle.X + (int)OFFSET.X, Rectangle.Y + (int)OFFSET.Y, Rectangle.Width, Rectangle.Height), 
                null,
                Color.Red,
                0,
                new(),
                0,
                //_position.Z
                0
                );
            spriteBatch.End();
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
                _position.Z = Cube.Z - size - 1f;
            }
            else if (_speed.Z < 0)
            {
                _position.Z = Cube.Z_OP + 1f;
                _speed.Z = 0;
            }
        }
        public void update(KeyboardState keyboardState, KeyboardState PreviouskeyboardState)
        {
            //_speed.Y += .4f;

            Vector2 normalizedSpeed = new();
            if (keyboardState.IsKeyDown((Keys)Game1.Keybind.Jump)) { _speed.Y -= 1; }
            if (keyboardState.IsKeyDown((Keys)Game1.Keybind.sneak)) { _speed.Y += 1; }
            //if (keyboardState.IsKeyDown((Keys)Game1.Keybind.up)) { normalizedSpeed.Y -= 1; }
            //if (keyboardState.IsKeyDown((Keys)Game1.Keybind.down)) { normalizedSpeed.Y += 1; }
            //if (keyboardState.IsKeyDown((Keys)Game1.Keybind.Left)) { normalizedSpeed.X -= 1; }
            //if (keyboardState.IsKeyDown((Keys)Game1.Keybind.Right)) { normalizedSpeed.X += 1; }
            MoveKeyPressed(keyboardState);
            normalizedSpeed = General.Normalize(normalizedSpeed, 1f);

            _speed.X += normalizedSpeed.X;
            _speed.Z += normalizedSpeed.Y;


            _speed.X *= .75f;
            _speed.Y *= .75f;
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
        enum Direction
        {
            UP = Keys.W,
            DOWN = Keys.S,
            LEFT = Keys.A,
            RIGHT = Keys.D
        }
        public void MoveKeyPressed(KeyboardState keyboardState)
        {
            Vector2 speedChange = new();

            if (keyboardState.IsKeyDown((Keys)Direction.UP))
            {
                speedChange += General.AngleToVector2(_angle.X);
            }
            if (keyboardState.IsKeyDown((Keys)Direction.DOWN))
            {
                speedChange -= General.AngleToVector2(_angle.X);
            }
            if (keyboardState.IsKeyDown((Keys)Direction.LEFT))
            {
                speedChange += General.AngleToVector2(_angle.X - (float)Math.PI / 2);
            }
            if (keyboardState.IsKeyDown((Keys)Direction.RIGHT))
            {
                speedChange += General.AngleToVector2(_angle.X + (float)Math.PI / 2);
            }

            if (speedChange != new Vector2(0, 0))
            {
                speedChange.Normalize();
                _speed.X += speedChange.X * 1;
                _speed.Z += speedChange.Y * 1;

            }
        }
        public Vector2 XY { get => new(_position.X, _position.Y); set; }
        public Vector3 Speed { get => _speed; set; }
        public Vector3 Position { get => _position; set; }
        public Rectangle Rectangle { get => new((int)_position.X, (int)_position.Y, size, size); set; }
        public Cube Cube { get => new(_position, size, size, size); set; } // working here


    }
}
