using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using System.IO;
using System.Text.RegularExpressions;

namespace tilemap_system
{
    internal class General
    {
        static Stopwatch _doubleClickTimer = new(250);

        public static Color Divide(Color color, float num) => new Color(color.R / num, color.G / num, color.B / num);
        public static Color Multiply(Color color, float num) => new Color(color.R * num, color.G * num, color.B * num);


        public static void DrawObject(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Point screenSize,
            float FOV,
            Vector3 cameraPosition,
            float pitch,
            float yaw,
            int width,
            int height,
            Rectangle? sourceRect,
            Color color,
            Vector3 objectPosition)
        {
            if (texture == null) return;

            Vector3 relativePos = objectPosition - cameraPosition;
            /*

            // Normalize once (avoid redundant sqrt later)
            float invLength = 1.0f / MathF.Sqrt(relativePos.X * relativePos.X +
                                                relativePos.Y * relativePos.Y +
                                                relativePos.Z * relativePos.Z);
            Vector3 dir = relativePos * invLength;
            
            // Yaw (horizontal angle)
            float objectYaw = MathF.Atan2(dir.X, dir.Z);

            // Pitch (vertical angle)
            float objectPitch = MathF.Asin(dir.Y);
            // 1. Calculate screen position
            float yawDifference = AngleDifference(yaw, objectYaw);
            float pitchDifference = AngleDifference(pitch, objectPitch);

            Point screenPos = new Point(
                (int)(Game1.screenSize.X / 2 + (-yawDifference / MathF.PI * FOV.X) * (Game1.screenSize.X / 2)),
                (int)(Game1.screenSize.Y / 2 + (-pitchDifference / MathF.PI * FOV.Y) * (Game1.screenSize.Y / 2))
            );
            */
            Vector3 rotatedrelativePos = RotateVector(relativePos, yaw, pitch);

            if (rotatedrelativePos.Z < 0) return; // Object is behind the camera, skip drawing
            //calculate screen Position
            float fov_scale = 1f / MathF.Tan(FOV / 2);
            Point screenPos = new Point(
                (int)((rotatedrelativePos.X / rotatedrelativePos.Z) * fov_scale * (screenSize.X / 2) + (screenSize.X / 2)),
                (int)((rotatedrelativePos.Y / rotatedrelativePos.Z) * fov_scale * (screenSize.Y / 2) + (screenSize.Y / 2))
            );

            // 2. Calculate size (world units to screen units)
            float scale = 50 / rotatedrelativePos.Z;
            int finalWidth = (int)(width * scale);
            int finalHeight = (int)(height * scale);

            if (finalWidth < 4 || finalHeight < 4) return;

            // 3. Center the object
            screenPos.X -= finalWidth / 2;
            screenPos.Y -= finalHeight / 2;

            // 4. Draw with depth-aware coloring
            spriteBatch.Draw(
                texture,
                new Rectangle(screenPos.X, screenPos.Y, finalWidth, finalHeight),
                sourceRect,
                color,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                DepthLayers.WorldMin + DepthLayers.WorldMax / Vector3.DistanceSquared(cameraPosition, objectPosition)
            );
        }
        public static Vector3 RotateVector(Vector3 vector, float yaw, float pitch)
        {
            // Yaw rotation (around Y axis) // first for fps feel
            float cosYaw = (float)Math.Cos(yaw);
            float sinYaw = (float)Math.Sin(yaw);
            float x1 = vector.X * cosYaw - vector.Z * sinYaw;
            float z1 = vector.X * sinYaw + vector.Z * cosYaw;

            // Pitch rotation (around X axis)
            float cosPitch = (float)Math.Cos(pitch);
            float sinPitch = (float)Math.Sin(pitch);
            float y1 = vector.Y * cosPitch - z1 * sinPitch;
            float z2 = vector.Y * sinPitch + z1 * cosPitch;

            return new Vector3(x1, y1, z2);
        }

        public static float AngleDifference(float a, float b)
        {
            float diff = a - b;
            while (diff > MathF.PI) diff -= MathF.Tau;
            while (diff < -MathF.PI) diff += MathF.Tau;
            return diff;
        }
        public static class IntDoubleArrayVisualizer
        {
            public static void VisualizeToFile(Chunk[,] inputArray, string fileName = "array_visualization.txt")
            {
                IntDouble[,] array = new IntDouble[inputArray.GetLength(0), inputArray.GetLength(1)];
                for (int x = 0; x < inputArray.GetLength(0); x++)
                {
                    for (int y = 0; y < inputArray.GetLength(1); y++)
                    {
                        if (inputArray[x, y] != null)
                        {
                            array[x, y] = inputArray[x, y]._chunkIndex;
                        }
                    }
                }

                // Get the MyDocuments folder path
                string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string filePath = Path.Combine(myDocuments, fileName);

                // Create the string visualization
                string visualization = VisualizeToString(array);

                // Write to file
                File.WriteAllText(filePath, visualization);

                Console.WriteLine($"Visualization saved to: {filePath}");
            }

            public static string VisualizeToString(IntDouble[,] array)
            {
                int rows = array.GetLength(0);
                int cols = array.GetLength(1);

                // Determine maximum widths for alignment
                int maxXWidth = 0;
                int maxZWidth = 0;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        var current = array[i, j];
                        maxXWidth = Math.Max(maxXWidth, current.X.ToString().Length);
                        maxZWidth = Math.Max(maxZWidth, current.Z.ToString().Length);
                    }
                }

                // Build the string representation
                StringBuilder sb = new();

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        var current = array[i, j];
                        sb.Append($"[{current.X.ToString().PadLeft(maxXWidth)},{current.Z.ToString().PadLeft(maxZWidth)}]");

                        // Add space between elements but not after last element in row
                        if (j < cols - 1) sb.Append(" ");
                    }

                    // Add newline except after last row
                    if (i < rows - 1) sb.AppendLine();
                }

                return sb.ToString();
            }
        }
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
            new Vector3(
            (float)(Math.Cos(angle.Y) * Math.Sin(angle.X)),
            (float)(Math.Sin(angle.Y)),
            (float)(Math.Cos(angle.Y) * Math.Cos(angle.X))
            );
        }
        public static bool OnDoubleClick(MouseState mouseState, MouseState previousMouseState)
        {
            if (OnLeftPress(mouseState, previousMouseState))
            {
                if (_doubleClickTimer.GetTimeMilliseconds() > 10 && !_doubleClickTimer.IsActive)
                {
                    _doubleClickTimer.Reset();
                    return true;
                }
                else
                {
                    _doubleClickTimer.Reset();
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static bool OnRightReleased(MouseState mouseState, MouseState previousMouseState)
        {
            return (mouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed);
        }
        public static bool OnRightPress(MouseState mouseState, MouseState previousMouseState)
        {
            return (mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released);
        }
        public static bool OnLeftReleased(MouseState mouseState, MouseState previousMouseState)
        {
            return (mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed);
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
            return new Color(color.R * num, color.G * num, color.B * num, color.A);
        }
        public static Vector2 ToVector2(Vector3 vector)
        {
            return new(vector.X, vector.Y);
        }
        public static float Vector2ToAngle(Vector2 angle)
        {
            return (float)Math.Atan2(angle.Y, angle.X);
        }
        public static Vector2 AngleToVector2(double angle)
        {
            return new((float)Math.Sin(angle), (float)Math.Cos(angle));
        }
    }
}
