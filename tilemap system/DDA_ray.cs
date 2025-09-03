using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using tilemap_system.tilemap_system;

namespace tilemap_system
{
    internal struct DDA_ray
    {
        //shoots 3 rays at once, one for each axis, each ray moves one tile in each respective axis.
        //every time Update is called, it moves the shortest ray 1 tile forward in its respective axis.
        //update returns the new position of the ray
        public Color _color { get; set; }
        Vector3 _origin;
        Vector3 Xpos, Ypos, Zpos;
        Vector3 X_Step, Y_Step, Z_Step;
        public DDA_ray(Vector3 position, Vector3 pos2)
        {
            _color = Color.Blue;
            _origin = Xpos = Ypos = Zpos = position;
            Vector3 _direction = General.Normalize(pos2 - position, 1);
            X_Step = new Vector3(1, _direction.Y / _direction.X, _direction.Z / _direction.X);
            if (_direction.X < 0) { X_Step *= -1; }
            Y_Step = new Vector3(_direction.X / _direction.Y, 1, _direction.Z / _direction.Y);
            if (_direction.Y < 0) { Y_Step *= -1; }
            Z_Step = new Vector3(_direction.X / _direction.Z, _direction.Y / _direction.Z, 1);
            if (_direction.Z < 0) { Z_Step *= -1; }

            // Moves the ray to the first edge of a tile it will hit
            // Then will move by exactly one tile in the direction of the ray
            if (_direction.X < 0) { Xpos += X_Step * (Xpos.X % Tile.Size) + _direction; }
            else { Xpos += X_Step * (Tile.Size - (Xpos.X % Tile.Size)) + _direction; }
            if (_direction.Y < 0) { Ypos += Y_Step * (Ypos.Y % Tile.Size) + _direction; }
            else { Ypos += Y_Step * (Tile.Size - (Ypos.Y % Tile.Size)) + _direction; }
            if (_direction.Z < 0) { Zpos += Z_Step * (Zpos.Z % Tile.Size) + _direction; }
            else { Zpos += Z_Step * (Tile.Size - (Zpos.Z % Tile.Size)) + _direction; }

            // Scale the steps to the size of a tile
            X_Step *= Tile.Size;
            Y_Step *= Tile.Size;
            Z_Step *= Tile.Size;
        }
        /* This constructor is not used, but it was part of the original code.
        public DDA_ray(Vector3 position, Vector2 angle)
        {
            _color = Color.Blue;
            _origin= Xpos = Ypos = Zpos = position;
            Vector3 _direction = new Vector3(
            (float)(Math.Cos(angle.Y) * Math.Sin(angle.X)),
            (float)(Math.Sin(angle.Y)),
            (float)(Math.Cos(angle.Y) * Math.Cos(angle.X))
                );
            X_Step = new Vector3(1, _direction.Y / _direction.X, _direction.Z / _direction.X);
            if (_direction.X < 0) X_Step *= -1;
            Y_Step = new Vector3(_direction.X / _direction.Y, 1, _direction.Z / _direction.Y);
            if (_direction.Y < 0) Y_Step *= -1;
            Z_Step = new Vector3(_direction.X / _direction.Z, _direction.Y / _direction.Z, 1);
            if (_direction.Z < 0) Z_Step *= -1;

            // Moves the ray to the first edge of a tile it will hit
            // Then will move by exactly one tile in the direction of the ray
            if (_direction.X < 0) Xpos += X_Step * (Xpos.X % Tile.XSize) + _direction;
            else Xpos += X_Step * (Tile.XSize - (Xpos.X % Tile.XSize)) + _direction;
            if (_direction.Y < 0) Ypos += Y_Step * (Ypos.Y % Tile.YSize) + _direction;
            else Ypos += Y_Step * (Tile.YSize - (Ypos.Y % Tile.YSize)) + _direction;
            if (_direction.Z < 0) Zpos += Z_Step * (Zpos.Z % Tile.ZSize) + _direction;
            else Zpos += Z_Step * (Tile.ZSize - (Zpos.Z % Tile.ZSize)) + _direction;
            X_Step *= Tile.XSize;
            Y_Step *= Tile.YSize;
            Z_Step *= Tile.ZSize;
        }
        */
        /* This method now incorperated into the constructor, but it was part of the original code.
        public void FirstMove(Vector3 _direction)
        {
            // Moves the ray to the first edge of a tile it will hit
            // Then will move by exactly one tile in the direction of the ray
            if (_direction.X < 0) Xpos += X_Step * (Xpos.X % Tile.XSize) + _direction;
            else Xpos += X_Step * (Tile.XSize - (Xpos.X % Tile.XSize)) + _direction;
            if (_direction.Y < 0) Ypos += Y_Step * (Ypos.Y % Tile.YSize) + _direction;
            else Ypos += Y_Step * (Tile.YSize - (Ypos.Y % Tile.YSize)) + _direction;
            if (_direction.Z < 0) Zpos += Z_Step * (Zpos.Z % Tile.ZSize) + _direction;
            else Zpos += Z_Step * (Tile.ZSize - (Zpos.Z % Tile.ZSize)) + _direction;
            X_Step *= Tile.XSize;
            Y_Step *= Tile.YSize;
            Z_Step *= Tile.ZSize;
        }
        */
        public Vector3 Update()
        {
            Vector3.DistanceSquared(ref _origin, ref Xpos, out float Xdistance);
            Vector3.DistanceSquared(ref _origin, ref Ypos, out float Ydistance);
            Vector3.DistanceSquared(ref _origin, ref Zpos, out float Zdistance);

            if (Xdistance <= Ydistance && Xdistance <= Zdistance)
            {
                _lowestDistanceSquared = Xdistance;
                Xpos += X_Step;
                return Xpos - X_Step;
            }
            else if (Ydistance <= Zdistance)
            {
                _lowestDistanceSquared = Ydistance;
                Ypos += Y_Step;
                return Ypos - Y_Step;
            }
            else 
            {
                _lowestDistanceSquared = Zdistance;
                Zpos += Z_Step;
                return Zpos - Z_Step;
            }
        }
        public static bool CheckLineOfSight(Vector3 pos1, Vector3 pos2)
        {
            DDA_ray ray = new DDA_ray(pos1, pos2);
            float Distance = Vector3.Distance(pos1, pos2);
            int renderDistance = (int)(Distance / Tile.Size) + 1; // +1 to ensure we check the last tile
            for (int l = 0; l < renderDistance; l++)
            {
                IntTriple TileIndex = Tile.GetTileIndex(ray.Update());
                if (World.GetTileCopyFromIndex(TileIndex, out Tile tile))
                {
                    if (tile.Isfull)
                    {
                        return false; // If we hit a full tile, return false
                    }
                }
                else
                {
                    return false; // If we could not retrieve the tile, return false [MAY CAUSE ERROS IF UNHANDLED]
                }
            }
            return true; // If we did not hit any full tiles, return true
        }

        float _lowestDistanceSquared;
        public readonly float LowestDistanceSquared { get => _lowestDistanceSquared; }
    }
}
