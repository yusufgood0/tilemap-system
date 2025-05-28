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
    public class General
    {
        public static bool OnPress(MouseState mouseState, MouseState previousMouseState)
        {
            return (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released);
        }
        public static bool OnRelease(MouseState mouseState, MouseState previousMouseState)
        {
            return (mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed);
        }
        public static bool OnPress(KeyboardState keyboardState, KeyboardState previousKeyboardState, Keys key)
        {
            return (keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key));
        }
        public static bool OnRelease(KeyboardState keyboardState, KeyboardState previousKeyboardState, Keys key)
        {
            return (keyboardState.IsKeyUp(key) && previousKeyboardState.IsKeyDown(key));
        }
        public static Vector2 Normalize(Vector2 vector, float hypotinuse)
        {
            float length = vector.X * vector.X + vector.Y * vector.Y;
            if (length > 0)
            {
                vector *= hypotinuse / MathF.Sqrt(length);
            }
            return vector;
        }
        public static Vector3 Normalize(Vector3 vector, float hypotenuse)
        {
            float length = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            if (length > 0)
            {
                vector *= hypotenuse / length;
            }            

            return vector;
        }

        public static Vector2 toVector2(Vector3 vector)
        {
            return new(vector.X, vector.Y);
        }
    }
}
