using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace tilemap_system
{
    internal struct IntDouble
    {
        public int X { get; set; }
        public int Z { get; set; }
        public IntDouble(IntTriple A)
        {
            X = A.X;
            Z = A.Z;
        }
        public IntDouble()
        {
            X = 0;
            Z = 0;
        }
        public IntDouble(int x, int z)
        {
            X = x;
            Z = z;
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return base.Equals(obj);
        }
        public static bool operator ==(IntDouble a, IntDouble b)
        {
            return (a.X == b.X && a.Z == b.Z);
        }
        public static bool operator !=(IntDouble a, IntDouble b)
        {
            return !(a.X == b.X && a.Z == b.Z);
        }
        public static IntDouble operator +(IntDouble a, IntDouble b)
        {
            return new IntDouble(a.X + b.X, a.Z + b.Z);
        }
        public static IntDouble operator -(IntDouble a)
        {
            return new IntDouble(-a.X, -a.Z);
        }
        public static IntDouble operator -(IntDouble a, IntDouble b)
        {
            return new IntDouble(a.X - b.X, a.Z - b.Z);
        }
        public static IntDouble operator *(IntDouble a, int b)
        {
            return new IntDouble(a.X * b, a.Z * b);
        }
        public static IntDouble operator %(IntDouble a, int b)
        {
            return new IntDouble(a.X % b, a.Z % b);
        }
        public static IntDouble operator /(IntDouble a, int b)
        {
            return new IntDouble(a.X / b, a.Z / b);
        }
        public bool inBound(IntDouble min, IntDouble max)
        {
            if (
                min.X < X && X < max.X &&
                min.Z < Z && Z < max.Z
                )
            {
                return true;
            }
            return false;
        }
        public IntDouble(Vector3 vector)
        {
            X = (int)vector.X;
            Z = (int)vector.Z;
        }

        public Vector2 XZ { get => new(X, Z); set; }
    }
}
