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
    internal class General
    {
        public static Vector2 Normalize(Vector2 vector, float hypotinuse)
        {
            //if ((vector.X > 1 && vector.X < -1) || (vector.Y > 1 && vector.Y < -1))
            if (vector.X != 0 || vector.Y != 0)
            {
                float num = hypotinuse / MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
                vector.X *= num;
                vector.Y *= num;
            }
            return vector;
        }
    }
}
