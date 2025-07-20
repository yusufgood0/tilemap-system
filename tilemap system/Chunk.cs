using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;

namespace tilemap_system
{
    internal class Chunk
    {
        public static readonly int _chunkSize = 16; // Size of the chunk (in tiles)
        public static readonly int _trueChunkSize = _chunkSize * Tile.XSize; // Size of the chunk (in units)
        public static readonly int _chunkHeight = 256; // Height of the chunk (in tiles)

        // for I/O operations
        //private static readonly string _archiveDirectory = Environment.CurrentDirectory;
        private static readonly string _archiveDirectory = Path.Combine(Environment.CurrentDirectory, "HomemadeMinecraft", "ChunkArchive");
        static readonly string tempPath = Path.Combine(_archiveDirectory, "tempChunk", "tempFile");

        //private static readonly StringBuilder _cachedStringBuilder = new StringBuilder(_chunkSize * _chunkHeight * _chunkSize * 2);


        Tile[,,] _Tiles = new Tile[_chunkSize, _chunkHeight, _chunkSize];
        public IntDouble _chunkIndex { get; set; }
        //public ref Tile[,,] GetTiles
        //{
        //    get { return ref _Tiles; }
        //}
        public void ArchiveChunk()
        {
            byte[] writeData = new byte[_chunkSize * _chunkHeight * _chunkSize];

            // Get tile type and convert to byte
            int i = 0;
            // loop x -> z -> y to match memory chache layout for efficiency with [,,] arrays
            for (int y = 0; y < _chunkHeight; y++)
                for (int z = 0; z < _chunkSize; z++)
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        writeData[i++] = (byte)_Tiles[x, y, z].getType;
                    }
            string filePath = Path.Combine(_archiveDirectory, $"C{_chunkIndex.X}_{_chunkIndex.Z}.txt");

            File.WriteAllBytes(filePath, writeData);
            Game1.Log($"successfully loaded chunk C{_chunkIndex.X}_{_chunkIndex.Z}.txt");

        }
        public static bool PullChunkFromArchive(IntDouble chunkIndex, out Chunk outputChunk)
        {
            outputChunk = new(chunkIndex, false);

            string filePath = Path.Combine(_archiveDirectory, $"C{chunkIndex.X}_{chunkIndex.Z}.txt");
            if (!File.Exists(filePath))
            {
                Game1.Log($"Chunk file C{chunkIndex.X}_{chunkIndex.Z} does not exist in archive.");
                return false; // Chunk does not exist in archive. Pull failed
            }
            byte[] data = File.ReadAllBytes(filePath);
            //File.Delete(filePath); // Delete the file after reading
            int i = 0;
            // loop x -> z -> y to match memory chache layout for efficiency with [,,] arrays
            for (int y = 0; y < _chunkHeight; y++)
                for (int z = 0; z < _chunkSize; z++)
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        outputChunk._Tiles[x, y, z] = new Tile((Tile.ID)data[i++]);
                    }
            Game1.Log($"Successfully pulled chunk C{chunkIndex.X}_{chunkIndex.Z} from archive.");
            return true; // Chunk successfully pulled from archive
        }

        public Chunk() { }
        public Chunk(IntDouble chunkIndex, bool GenerateTiles)
        {
            _chunkIndex = chunkIndex;
            if (GenerateTiles)
            {
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
