using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal struct Tile
    {
        static readonly int maxHealth = 100;
        static readonly int _size = 40;

        TileID _type;
        public enum TileID : byte
        {
            Empty = 0,
            Grass = 1,
            Stone = 2,
            DiamondBlock = 3,
        }
        public Tile()
        {
            _type = TileID.Empty;
        }
        public Tile(TileID type)
        {
            _type = type;
        }
        public ref Tile this[IntTriple index] { get => ref this[index.X, index.Y, index.Z]; }
        public ref Tile this[int z, int y, int x] => ref this[x, y, z];

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
        public static IntTriple GetTileIndex(Vector3 worldPosition)
        {
            return new IntTriple(
                (int)(worldPosition.X / _size),
                (int)(worldPosition.Y / _size),
                (int)(worldPosition.Z / _size)
                );
        }

        public static bool IsCollision(Cube cube)
        {
            IntTriple index1 = GetTileIndex(new Vector3(cube.X, cube.Y, cube.Z));
            IntTriple index2 = GetTileIndex(new Vector3(cube.X_OP, cube.Y_OP, cube.Z_OP));

            for (int x = index1.X; x < index2.X + 1; x++)
                for (int y = index1.Y; y < index2.Y + 1; y++)
                    for (int z = index1.Z; z < index2.Z + 1; z++)
                    {
                        if (World.GetTileCopyFromIndex(new(x, y, z), out Tile tile) && tile.Isfull)
                        {
                            return true;
                        }
                    }

            return false;
        }
        public static List<IntTriple> CollidingTilesTriple(Cube cube)
        {
            List<IntTriple> Indeces = new();

            IntTriple index1 = GetTileIndex(new Vector3(cube.X, cube.Y, cube.Z));
            IntTriple index2 = GetTileIndex(new Vector3(cube.X_OP, cube.Y_OP, cube.Z_OP));

            for (int x = index1.X; x < index2.X + 1; x++)
                for (int y = index1.Y; y < index2.Y + 1; y++)
                    for (int z = index1.Z; z < index2.Z + 1; z++)
                    {
                        Indeces.Add(new IntTriple(x, y, z));
                    }
            return Indeces;
        }
        /*
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
        */
        public bool MineTile(int TimeMined, Item item, out float minePercentage) //in Milliseconds
        {
            if (!TileInfo.TileDictionary[(int)_type].TierRequirmentsMet(item.Tier))
            {
                TimeMined /= 2; //if the item tier is lower than the tile tier, it takes twice as long to mine
            }
            else if (item.Proficiencies != null && TileInfo.TileDictionary[(int)_type].HasProficiency((ToolProficiency)item.Proficiencies))
            {
                TimeMined = (int)(TimeMined * (float)item.MineSpeedBonus);
            }

            if (GetTileInfo.GetMineTime() < TimeMined)
            {
                _type = TileID.Empty;
                minePercentage = 1; //100% mined
                return true; //tile broke
            }
            minePercentage = (float)TimeMined / GetTileInfo.GetMineTime();
            return false; //tile did not break
        }
        public void SetType(TileID type)
        {
            _type = type;
        }

        public static Cube GetCube(int Xindex, int Yindex, int Zindex) => new Cube(GetPosition(Xindex, Yindex, Zindex), Size);
        public static Cube GetCube(IntTriple index) => new Cube(GetPosition(index.X, index.Y, index.Z), Size, Size, Size);
        public static IntTriple GetPosition(int Xindex, int Yindex, int Zindex) => new IntTriple(Xindex * Size, Yindex * Size, Zindex * Size);
        static public int Size { get => _size; }
        public bool Isfull { get => !(_type == TileID.Empty); set; }
        new public TileID GetType { get => _type; }
        public Color Color { get => GetTileInfo.GetTexture(); }
        public ref TileInfo GetTileInfo { get => ref TileInfo.TileDictionary[(int)_type]; }
    }
}
