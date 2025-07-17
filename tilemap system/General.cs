using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;

namespace tilemap_system
{
    internal class General
    {
        public static void Bound(ref float value, float maxValue) //handles overflows of a value by setting it back to zero, and zero to the maxvalue
        {
            value = (value % maxValue + maxValue) % maxValue;
        }
        public static int Bound(int value, int maxValue) //handles overflows of a value by setting it back to zero, and zero to the maxvalue
        {
            return (value % maxValue + maxValue) % maxValue;
        }
        public static float ToRadians(float degrees)
        {
            return degrees * (MathF.PI / 180f);
        }
        
        public static Vector3 rotate(Vector3 Vector, float yaw, float pitch)
        {
            // Yaw: rotate around Y-axis
            float cosYaw = (float)Math.Cos(yaw);
            float sinYaw = (float)Math.Sin(yaw);
            float x1 = Vector.X * cosYaw + Vector.Z * sinYaw;
            float z1 = -Vector.X * sinYaw + Vector.Z * cosYaw;

            // Pitch: rotate around X-axis
            float cosPitch = (float)Math.Cos(pitch);
            float sinPitch = (float)Math.Sin(pitch);
            float y1 = Vector.Y * cosPitch - z1 * sinPitch;
            float z2 = Vector.Y * sinPitch + z1 * cosPitch;

            Vector.X = x1;
            Vector.Y = y1;
            Vector.Z = z2;

            return Vector;
        }
        public static Vector3 angleToVector3(Vector2 angle)
        {
            return 
            General.Normalize(new Vector3(
            (float)(Math.Cos(angle.Y) * Math.Sin(angle.X)),
            (float)(Math.Sin(angle.Y)),
            (float)(Math.Cos(angle.Y) * Math.Cos(angle.X))
            ), 1);
        }
        public static bool OnRightPress(MouseState mouseState, MouseState previousMouseState)
        {
            return (mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released);
        }
        public static bool OnLeftPress(MouseState mouseState, MouseState previousMouseState)
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
        
        public static IntTriple Clamp(IntTriple Triple, IntTriple min, IntTriple max)
        {
            return new IntTriple(
                Math.Clamp(Triple.X, min.X, max.X - 1),
                Math.Clamp(Triple.Y, min.Y, max.Y - 1),
                Math.Clamp(Triple.Z, min.Z, max.Z - 1)
                );
        }
        public static Color colorMultiply(Color color, float num)
        {
            return new Color (color.R * num, color.G * num, color.B * num, color.A);
        }
        public static Vector2 ToVector2(Vector3 vector)
        {
            return new(vector.X, vector.Y);
        }
        public static Vector2 AngleToVector2(double angle)
        {
            Vector2 Vector = new((float)Math.Sin(angle), (float)Math.Cos(angle));
            Vector.Normalize();
            return Vector;
        }
    }
}
