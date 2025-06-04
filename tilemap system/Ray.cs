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
    internal class Ray
    {
        private Vector3 _direction;
        private Vector3 _origin;
        public Vector3 _position;
        public Color _color { get; set; }
        public Ray(Vector2 angle, Vector3 position)
        {
            _origin = position;
            _position = position;
            _direction = General.Normalize(new Vector3(
                (float)(Math.Cos(angle.Y) * Math.Sin(angle.X)),
                (float)(Math.Sin(angle.Y)),
                (float)(Math.Cos(angle.Y) * Math.Cos(angle.X))
                ), 1);
            //_direction = new(1, 1, 1);
        }
       
        public Ray(Vector3 direction, Vector3 position)
        {
            _position = position;
            _direction = direction;
        }
        public void update(int travelDistance)
        {
            _position += _direction * travelDistance;
        }
        public void reverse(int travelDistance)
        {
            _position -= _direction * travelDistance;
        }
        public float DistanceTraveled { get => Vector3.Distance(_origin, _position); set; }
    }
}
