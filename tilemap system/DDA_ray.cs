using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace tilemap_system
{
    internal class DDA_ray
    {
        private Vector3 _direction;
        private Vector3 _origin;
        public Vector3 _position;
        public Vector3 Xpos;
        public Vector3 Ypos;
        public Vector3 Zpos;
        public Vector3 X_direction;
        public Vector3 Y_direction;
        public Vector3 Z_direction;

        public Color _color { get; set; }
        public DDA_ray(Vector2 angle, Vector3 position)
        {
            _origin = position;
            _position = position;
            Xpos = _position;
            Ypos = _position;
            Zpos = _position;
            _direction = General.Normalize(new Vector3(
            (float)(Math.Cos(angle.Y) * Math.Sin(angle.X)),
            (float)(Math.Sin(angle.Y)),
            (float)(Math.Cos(angle.Y) * Math.Cos(angle.X))
                ), 1);
            X_direction = new Vector3(1, _direction.Y / _direction.X, _direction.Z / _direction.X);
            if (_direction.X < 0) X_direction *= -1;
            Y_direction = new Vector3(_direction.X / _direction.Y, 1, _direction.Z / _direction.Y);
            if (_direction.Y < 0) Y_direction *= -1;
            Z_direction = new Vector3(_direction.X / _direction.Z, _direction.Y / _direction.Z, 1);
            if (_direction.Z < 0) Z_direction *= -1;
            FirstMove();
        }

        public void FirstMove()
        {
            if (_direction.X < 0) Xpos += X_direction *                (Xpos.X % Tiles.XSize)   + _direction;
            else                  Xpos += X_direction * (Tiles.XSize - (Xpos.X % Tiles.XSize))  + _direction;
            if (_direction.Y < 0) Ypos += Y_direction *                (Ypos.Y % Tiles.YSize)   + _direction;
            else                  Ypos += Y_direction * (Tiles.YSize - (Ypos.Y % Tiles.YSize))  + _direction;
            if (_direction.Z < 0) Zpos += Z_direction *                (Zpos.Z % Tiles.ZSize)   + _direction;
            else                  Zpos += Z_direction * (Tiles.ZSize - (Zpos.Z % Tiles.ZSize))  + _direction;
            X_direction *= Tiles.XSize;
            Y_direction *= Tiles.YSize;
            Z_direction *= Tiles.ZSize;
        }
        public Vector3 Update()
        {
            float Xdistance = Vector3.Distance(_origin, Xpos);
            float Ydistance = Vector3.Distance(_origin, Ypos);
            float Zdistance = Vector3.Distance(_origin, Zpos);
            lowestDistance = Math.Min(Math.Min(Xdistance, Ydistance), Zdistance);

            Vector3 returnValue;
            if (lowestDistance == Xdistance)
            {
                returnValue = Xpos;
                Xpos += X_direction;
            }
            else if (lowestDistance == Ydistance)
            {
                returnValue = Ypos;
                Ypos += Y_direction;
            }
            else 
            {
                returnValue = Zpos;
                Zpos += Z_direction;
            }
            return returnValue;
        }
        public float lowestDistance { get; set; }
    }
}
