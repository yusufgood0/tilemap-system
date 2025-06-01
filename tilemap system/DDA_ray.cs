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
            Y_direction = new Vector3(_direction.X / _direction.Y, 1, _direction.Z / _direction.Y);
            Z_direction = new Vector3(_direction.X / _direction.Z, _direction.Y / _direction.Z, 1);
        }
        public Vector3 FirstMove()
        {
            Xpos += X_direction * (Tiles.XSize - Xpos.X % Tiles.XSize);
            Ypos += Y_direction * (Tiles.YSize - Ypos.Y % Tiles.YSize);
            Zpos += Z_direction * (Tiles.ZSize - Zpos.X % Tiles.ZSize);
            X_direction *= Tiles.XSize;
            Y_direction *= Tiles.YSize;
            Z_direction *= Tiles.ZSize;

            float Xdistance = Vector3.Distance(_origin, Xpos);
            float Ydistance = Vector3.Distance(_origin, Ypos);
            float Zdistance = Vector3.Distance(_origin, Zpos);
            float lowest = Math.Min(Math.Min(Xdistance, Ydistance), Zdistance);

            Vector3 returnValue;
            switch (lowest)
            {
                case float value when value == Xdistance:
                    Xpos += X_direction;
                    returnValue = Xpos; break;
                case float value when value == Ydistance:
                    Ypos += Y_direction;
                    returnValue = Ypos; break;
                default:
                    Zpos += Z_direction;
                    returnValue = Zpos; break;
            }
            return returnValue;
        }
        public Vector3 Update()
        {
            float Xdistance = Vector3.Distance(_origin, Xpos);
            float Ydistance = Vector3.Distance(_origin, Ypos);
            float Zdistance = Vector3.Distance(_origin, Zpos);
            float lowest = Math.Min(Math.Min(Xdistance, Ydistance), Zdistance);

            Vector3 returnValue;
            switch (lowest)
            {
                case float value when value == Xdistance:
                    Xpos += X_direction;
                    returnValue = Xpos; break;
                case float value when value == Ydistance:
                    Ypos += Y_direction;
                    returnValue = Ypos; break;
                default:
                    Zpos += Z_direction;
                    returnValue = Zpos; break;
            }
            return returnValue;
        }
    }
}
