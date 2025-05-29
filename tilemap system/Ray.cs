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
    internal struct Ray
    {
        private Vector3 _direction;
        private float _distanceTraveled = 0;
        private Vector3 _position;
        public Ray(Vector2 angle, Vector3 position)
        {
            _position = position;
            _direction = new Vector3(
                (float)(Math.Cos(angle.Y) * Math.Cos(angle.X)),
                (float)Math.Sin(angle.Y),
                (float)(Math.Cos(angle.Y) * Math.Sin(angle.X)));
        }
        public Ray(Vector3 direction, Vector3 position)
        {
            _position = position;
            _direction = direction;
        }

        public IntTriple update(int travelDistance)
        {
            _distanceTraveled += travelDistance;
            _position += General.Normalize(_direction, travelDistance);
            return new(_position);
        }
    }
}
