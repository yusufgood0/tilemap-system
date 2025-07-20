using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tilemap_system.tilemap_system;

namespace tilemap_system
{
    internal class Tile
    {
        public static Texture2D _texture;
        static readonly int maxHealth = 100;
        static readonly int xSize = 40;
        static readonly int ySize = 40;
        static readonly int zSize = 40;

        //IntTriple _position = new();
        int _health = maxHealth;
        ID _type;
        public enum ID : byte
        {
            Empty = 0,
            Grass = 1,
            Stone = 2,
        }
        public Tile()
        {
            _type = ID.Empty;
        }
        public Tile(ID type)
        {
            _type = type;
        }
        public ref Tile this[IntTriple index] { get => ref this[index.X, index.Y, index.Z]; }
        public ref Tile this[int z, int y, int x] => ref this[x, y, z];

        public static void SetTexture(Texture2D texture)
        {
            _texture = texture;
        }
        /* a relic of old code
        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(_texture,
                    new Rectangle(
                        _position.X + (int)offset.X,
                        _position.Y + (int)offset.Y,
                        XSize,
                        YSize),
                    null,
                    TileInfo._tileInfo[(int)_type].getTexture(),
                    0,
                    new(),
                    0,
                    //_collideCube.Z
                    0
                    );
            spriteBatch.End();
        }// Draws the tile on the screen with the given offset
        */
        public static IntTriple getTileIndex(Vector3 worldPosition)
        {
            int x = (int)Math.Floor(worldPosition.X / xSize);
            int y = (int)Math.Floor(worldPosition.Y / ySize);
            int z = (int)Math.Floor(worldPosition.Z / zSize);
            return new IntTriple(x, y, z);
        }
        public static IntTriple getTileIndex(IntTriple position)
        {
            return new IntTriple(
                position.X / xSize,
                position.Y / ySize,
                position.Z / zSize
                );
        }

        public static Tile getTile(Vector3 position, Tile[,,] _Tiles)
        {
            int x = (int)(position.X / xSize);
            int y = (int)(position.Y / ySize);
            int z = (int)(position.Z / zSize);
            return _Tiles[x, y, z];
        }
        public static bool IsCollision(Cube cube)
        {
            IntTriple index1 = getTileIndex(new Vector3(cube.X, cube.Y, cube.Z));
            IntTriple index2 = getTileIndex(new Vector3(cube.X_OP, cube.Y_OP, cube.Z_OP));

            for (int x = index1.X; x < index2.X + 1; x++)
                for (int y = index1.Y; y < index2.Y + 1; y++)
                    for (int z = index1.Z; z < index2.Z + 1; z++)
                    {
                        if (World.GetTileFromIndex(new(x, y, z), out Tile tile))
                        {
                            if (tile.Isfull)
                            {
                                return true;
                            }
                        }
                    }

            return false;
        }
        public static List<IntTriple> CollidingTiles(Cube cube)
        {
            List<IntTriple> Indeces = new List<IntTriple>();

            IntTriple index1 = getTileIndex(new Vector3(cube.X, cube.Y, cube.Z));
            IntTriple index2 = getTileIndex(new Vector3(cube.X_OP, cube.Y_OP, cube.Z_OP));

            for (int x = index1.X; x < index2.X + 1; x++)
                for (int y = index1.Y; y < index2.Y + 1; y++)
                    for (int z = index1.Z; z < index2.Z + 1; z++)
                    {
                        Indeces.Add(new IntTriple(x, y, z));
                    }

            return Indeces;
        }
        public static List<Tile> getLoaded(Vector3 focusPoint, IntTriple range, IntTriple TileArray, Tile[,,] _Tiles)
        {
            IntTriple CameraTileIndex = Tile.getTileIndex(focusPoint);
            List<Tile> tiles = new List<Tile>();
            range = new(
                (int)Math.Round((float)range.X / xSize) + 1,
                (int)Math.Round((float)range.Y / ySize) + 1,
                (int)Math.Round((float)range.Z / ZSize) + 1
                );

            for (int x = Math.Max(CameraTileIndex.X - range.X, 0); x < Math.Min(CameraTileIndex.X + range.X + 1, TileArray.X); x++)
                for (int y = Math.Max(CameraTileIndex.Y - range.Y, 0); y < Math.Min(CameraTileIndex.Y + range.Y + 1, TileArray.Y); y++)
                    for (int z = Math.Max(CameraTileIndex.Z - range.Z, 0); z < Math.Min(CameraTileIndex.Z + range.Z + 1, TileArray.Z); z++)
                        if (TileArray.X > x && x > 0 && TileArray.Y > y && y > 0 && TileArray.Z > z && z > 0)
                        {
                            tiles.Add(_Tiles[x, y, z]);
                        }

            return tiles;
        }
        //public void UpdateTexture() { }

        public void Heal()
        {
            _health = maxHealth;
        }
        public void MineTile(int damage)
        {
            _health = _health - damage;
            if (_health < 0)
            {
                _health = 0;
                _type = ID.Empty;
            }
        }
        public void setType(ID type)
        {
            _type = type;
        }
        public static Cube GetCube(int Xindex, int Yindex, int Zindex) { return new Cube(GetPosition(Xindex, Yindex, Zindex), XSize, YSize, ZSize); }
        public static Cube GetCube(IntTriple index) { return new Cube(GetPosition(index.X, index.Y, index.Z), XSize, YSize, ZSize); }
        public static IntTriple GetPosition(int Xindex, int Yindex, int Zindex) { return new IntTriple(Xindex * XSize, Yindex * ySize, Zindex * zSize); }
        static public int XSize { get => xSize; }
        static public int YSize { get => ySize; }
        static public int ZSize { get => zSize; }
        public bool Isfull { get => !(_type == ID.Empty); set; }
        public ID getType { get => _type; }
        public Color color { get => TileInfo._tileInfo[(int)_type].getTexture(); }
    }
}
