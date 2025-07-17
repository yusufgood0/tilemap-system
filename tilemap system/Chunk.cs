using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace tilemap_system
{
    internal class Chunk
    {
        public static readonly int _chunkSize = 16; // Size of the chunk (in tiles)
        public static readonly int _trueChunkSize = _chunkSize * Tile.XSize; // Size of the chunk (in units)
        public static readonly int _chunkHeight = 200; // Height of the chunk (in tiles)
        Tile[,,] _Tiles = new Tile[_chunkSize, _chunkHeight, _chunkSize];
        public ref Tile[,,] GetTiles
        {
            get { return ref _Tiles; }
        }
        public Chunk(IntDouble chunkIndex)
        {
            // Calculate world-space chunk origin (in world units)
            int chunkWorldX = chunkIndex.X * _trueChunkSize;
            int chunkWorldZ = chunkIndex.Z * _trueChunkSize;

            for (int localX = 0; localX < _chunkSize; localX++)
            {
                for (int localY = 0; localY < _chunkHeight; localY++)
                {
                    for (int localZ = 0; localZ < _chunkSize; localZ++)
                    {
                        // Calculate world position in units (not tiles)
                        int worldX = chunkWorldX + localX * Tile.XSize;
                        int worldY = localY * Tile.YSize;
                        int worldZ = chunkWorldZ + localZ * Tile.ZSize;

                        // Determine tile type based on height
                        Tile.ID type = localY < 100 ? Tile.ID.Empty : (localY % 2 == 1 ? Tile.ID.Grass : Tile.ID.Stone);

                        // Create tile with proper world coordinates
                        _Tiles[localX, localY, localZ] = new Tile(worldX, worldY, worldZ, type);
                    }
                }
            }
        }
        public ref Tile getTile(Vector3 worldPos)
        {
            // Calculate local tile indices with proper negative handling
            int indexX = (int)((worldPos.X % _trueChunkSize + _trueChunkSize) % _trueChunkSize) / Tile.XSize;
            int indexY = (int)Math.Clamp(worldPos.Y / Tile.YSize, 0, _chunkHeight - 1);
            int indexZ = (int)((worldPos.Z % _trueChunkSize + _trueChunkSize) % _trueChunkSize) / Tile.ZSize;

            return ref _Tiles[indexX, indexY, indexZ];
        }
        public ref Tile getTileFromWorldIndex(IntTriple index)
        {
            index.X %= _chunkSize;
            index.Z %= _chunkSize;

            return ref _Tiles[1, Math.Clamp(index.Y, 1, Chunk._chunkHeight - 1), 1];
        }
    }
}
