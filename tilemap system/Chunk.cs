using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        public static readonly int _chunkHeight = 256; // Height of the chunk (in tiles)

        // for I/O operations
        //private static readonly string _archiveDirectory = Environment.CurrentDirectory;
        private static readonly string _archiveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //private static readonly StringBuilder _cachedStringBuilder = new StringBuilder(_chunkSize * _chunkHeight * _chunkSize * 2);


        Tile[,,] _Tiles = new Tile[_chunkSize, _chunkHeight, _chunkSize];
        public IntDouble _chunkIndex { get; set; }
        //public ref Tile[,,] GetTiles
        //{
        //    get { return ref _Tiles; }
        //}
        public void ArchiveChunk()
        {
            byte[] data = new byte[_chunkSize * _chunkHeight * _chunkSize];

            // Get tile type and convert to byte
            int i = 0;
            // loop x -> z -> y to match memory chache layout for efficiency with [,,] arrays
            for (int y = 0; y < _chunkHeight; y++)
                for (int z = 0; z < _chunkSize; z++)
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        data[i++] = (byte)_Tiles[x, y, z].getType;
                    }
            string filePath = Path.Combine(_archiveDirectory, $"C{_chunkIndex.X}_{_chunkIndex.Z}");
            File.WriteAllBytes(filePath, data);
        }
        public static bool PullChunkFromArchive(IntDouble chunkIndex, out Chunk outputChunk)
        {
            outputChunk = new();

            string filePath = Path.Combine(_archiveDirectory, $"C{chunkIndex.X}_{chunkIndex.Z}");
            if (!File.Exists(filePath))
                return false; // Chunk does not exist in archive. Pull failed
            byte[] data = File.ReadAllBytes(filePath);

            int i = 0;
            // loop x -> z -> y to match memory chache layout for efficiency with [,,] arrays
            for (int y = 0; y < _chunkHeight; y++)
                for (int z = 0; z < _chunkSize; z++)
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        outputChunk._Tiles[x, y, z] = new Tile((Tile.ID)data[i++]);
                    }
            return true; // Chunk successfully pulled from archive
        }

        public Chunk() { }
        public Chunk(IntDouble chunkIndex)
        {
            _chunkIndex = chunkIndex;

            // Calculate world-space chunk origin (in world units)
            int chunkWorldX = chunkIndex.X * _trueChunkSize;
            int chunkWorldZ = chunkIndex.Z * _trueChunkSize;

            for (int localX = 0; localX < _chunkSize; localX++)
            {
                for (int localY = 0; localY < _chunkHeight; localY++)
                {
                    for (int localZ = 0; localZ < _chunkSize; localZ++)
                    {
                        // Determine tile type based on height
                        Tile.ID type;
                        if (localY < 100)
                        {
                            type = Tile.ID.Empty; // Below height 100, use empty tile
                        }
                        else if (localY < 150)
                        {
                            type = Tile.ID.Grass; // Between height 100 and 150, use grass tile
                        }
                        else
                        {
                            type = Tile.ID.Stone; // Above height 150, use stone tile
                        }
                        // Create tile with proper world coordinates
                        _Tiles[localX, localY, localZ] = new Tile(type);
                    }
                }
            }
        }
        public ref Tile getTile(Vector3 worldPos)
        {
            // Calculate local tile indices with proper wrapping for negatives
            int indexX = (int)(worldPos.X / Tile.XSize) % _chunkSize;
            int indexZ = (int)(worldPos.Z / Tile.ZSize) % _chunkSize;

            // Ensure indices are positive (handles negative modulo results)
            if (indexX < 0) indexX += _chunkSize;
            if (indexZ < 0) indexZ += _chunkSize;

            // Clamp Y to valid range
            int indexY = (int)Math.Clamp(worldPos.Y / Tile.YSize, 0, _chunkHeight - 1);

            return ref _Tiles[indexX, indexY, indexZ];
        }
        public ref Tile getTileFromWorldIndex(IntTriple index)
        {
            // Wrap X and Z indices to stay within chunk bounds
            int x = index.X % _chunkSize;
            int z = index.Z % _chunkSize;

            // Ensure positive indices
            if (x < 0) x += _chunkSize;
            if (z < 0) z += _chunkSize;

            // Clamp Y to valid range (0 to _chunkHeight - 1)
            int y = Math.Clamp(index.Y, 0, _chunkHeight - 1);

            return ref _Tiles[x, y, z];
        }
    }
}
