using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace tilemap_system
{
    namespace tilemap_system
    {
        internal class World
        {
            private static int renderDistance = 16;


            private static readonly int _loadedChunksSize = renderDistance * 2 + 1;
            private static Chunk[,] _LoadedChunks = new Chunk[_loadedChunksSize, _loadedChunksSize];
            private static List<IntDouble> _IndecesToLoad = new();
            private static List<Chunk> _ChunksToArchive = new();
            private static object _loadedChunkCenterlock = new(); // Lock for thread safety
            private static IntDouble _loadedChunkCenter;
            //cached values
            private static int lastIndex = _loadedChunksSize - 1;


            //for shifting method
            private static IntDouble[] indicesToAdd = new IntDouble[_loadedChunksSize];
            static Chunk[] bufferChunksToArchive = new Chunk[_loadedChunksSize];
            static IntDouble[] bufferIndecesToLoad = new IntDouble[_loadedChunksSize];
            public static void SaveLoadedChunks()
            {
                lock (_LoadedChunks)
                {
                    for (int xIndex = 0; xIndex < lastIndex; xIndex++)
                    {
                        for (int yIndex = 0; yIndex < _loadedChunksSize; yIndex++)
                        {
                            if (_LoadedChunks[xIndex, yIndex] == null)
                            {
                                Game1.Log("NULL CHUNK SAVE DETECTED!");
                            }
                            _LoadedChunks[xIndex, yIndex].ArchiveChunk();
                        }
                    }
                }
                Game1.Log("Saved all chunks to archive");
            }
            public static void Initilize(Vector3 FocusPosition)
            {
                lock (_loadedChunkCenterlock)
                {
                    _loadedChunkCenter = GetChunkIndex(FocusPosition);
                }
                lock (_LoadedChunks)
                {
                    Chunk chunk;
                    for (int x = 0; x < _loadedChunksSize; x++)
                    {
                        for (int y = 0; y < _loadedChunksSize; y++)
                        {
                            IntDouble worldIndex = ChunksIndexToWorldIndex(new(x, y));
                            if (Chunk.PullChunkFromArchive(worldIndex, out chunk))
                            {
                                _LoadedChunks[x, y] = chunk;
                            }
                            else
                            {
                                _LoadedChunks[x, y] = new Chunk(worldIndex, true);
                            }
                        }
                    }
                }
            }
            public static void ShiftLoadedChunksLeft()
            {
                lock (_loadedChunkCenterlock)
                {
                    _loadedChunkCenter.X -= 1;
                }
                lock (_LoadedChunks)
                {

                    for (int yIndex = 0; yIndex < _loadedChunksSize; yIndex++)
                    {
                        bufferChunksToArchive[yIndex] = _LoadedChunks[0, yIndex];
                    }
                    //Array.Copy(_LoadedChunks, yIndex * _loadedChunksSize + 1, _LoadedChunks, yIndex * _loadedChunksSize, lastIndex);
                    for (int xIndex = 0; xIndex < lastIndex; xIndex++)
                    {
                        for (int yIndex = 0; yIndex < _loadedChunksSize; yIndex++)
                        {
                            _LoadedChunks[xIndex, yIndex] = _LoadedChunks[xIndex + 1, yIndex];
                        }
                    }
                    for (int yIndex = 0; yIndex < _loadedChunksSize; yIndex++)
                    {
                        _LoadedChunks[lastIndex, yIndex] = null;
                        bufferIndecesToLoad[yIndex] = new IntDouble(lastIndex, yIndex);
                    }
                }
                lock (_ChunksToArchive)
                {
                    _ChunksToArchive.AddRange(bufferChunksToArchive);
                }
                lock (_IndecesToLoad)
                {
                    _IndecesToLoad.AddRange(bufferIndecesToLoad);
                }
            }
            public static void ShiftLoadedChunksRight()
            {
                lock (_loadedChunkCenterlock)
                {
                    _loadedChunkCenter.X += 1;
                }
                lock (_LoadedChunks)
                {

                    for (int yIndex = 0; yIndex < _loadedChunksSize; yIndex++)
                    {
                        bufferChunksToArchive[yIndex] = _LoadedChunks[lastIndex, yIndex];
                    }
                    for (int xIndex = lastIndex; xIndex > 1; xIndex--)
                    {
                        for (int yIndex = 0; yIndex < _loadedChunksSize; yIndex++)
                        {
                            _LoadedChunks[xIndex, yIndex] = _LoadedChunks[xIndex - 1, yIndex];
                        }
                    }
                    for (int yIndex = 0; yIndex < _loadedChunksSize; yIndex++)
                    {
                        _LoadedChunks[0, yIndex] = null;
                        bufferIndecesToLoad[yIndex] = new IntDouble(0, yIndex);
                    }
                }
                lock (_ChunksToArchive)
                {
                    _ChunksToArchive.AddRange(bufferChunksToArchive);
                }
                lock (_IndecesToLoad)
                {
                    _IndecesToLoad.AddRange(bufferIndecesToLoad);
                }
            }
            public static void ShiftLoadedChunksDown()
            {
                lock (_loadedChunkCenterlock)
                {
                    _loadedChunkCenter.Z -= 1;
                }
                lock (_LoadedChunks)
                {
                    for (int xIndex = 0; xIndex < _loadedChunksSize; xIndex++)
                    {
                        bufferChunksToArchive[xIndex] = _LoadedChunks[xIndex, lastIndex];

                        for (int yIndex = 0; yIndex < lastIndex; yIndex++)
                        {
                            //Array.Copy(_LoadedChunks, (yIndex + 1) * _loadedChunksSize, _LoadedChunks, yIndex * _loadedChunksSize, lastIndex);
                            _LoadedChunks[xIndex, yIndex] = _LoadedChunks[xIndex, yIndex + 1];
                        }
                        _LoadedChunks[xIndex, 0] = null;
                        bufferIndecesToLoad[xIndex] = new IntDouble(xIndex, 0);
                    }
                }
                lock (_ChunksToArchive)
                {
                    _ChunksToArchive.AddRange(bufferChunksToArchive);
                }
                lock (_IndecesToLoad)
                {
                    _IndecesToLoad.AddRange(bufferIndecesToLoad);
                }
            }
            public static void ShiftLoadedChunksUp()
            {
                lock (_loadedChunkCenterlock)
                {
                    _loadedChunkCenter.Z += 1;
                }
                lock (_LoadedChunks)
                {
                    for (int xIndex = 0; xIndex < _loadedChunksSize; xIndex++)
                    {
                        bufferChunksToArchive[xIndex] = _LoadedChunks[xIndex, 0];
                        for (int yIndex = lastIndex; yIndex > 1; yIndex--)
                        {
                            //Array.Copy(_LoadedChunks, yIndex * _loadedChunksSize, _LoadedChunks, (yIndex + 1) * _loadedChunksSize, lastIndex);
                            _LoadedChunks[xIndex, yIndex] = _LoadedChunks[xIndex, yIndex - 1];
                        }
                        _LoadedChunks[xIndex, lastIndex] = null;
                        bufferIndecesToLoad[xIndex] = new IntDouble(xIndex, lastIndex);
                    }
                }
                lock (_ChunksToArchive)
                {
                    _ChunksToArchive.AddRange(bufferChunksToArchive);
                }
                lock (_IndecesToLoad)
                {
                    _IndecesToLoad.AddRange(bufferIndecesToLoad);
                }
            }
            public static void ShiftChunks(Vector3 FocusPosition)
            {
                IntDouble newLoadedChunkCenter = GetChunkIndex(FocusPosition);
                if (_loadedChunkCenter == newLoadedChunkCenter) { return; } // Check if the center chunk has changed

                int xShift = newLoadedChunkCenter.X - _loadedChunkCenter.X;
                int zShift = newLoadedChunkCenter.Z - _loadedChunkCenter.Z;
                for (int i = 0; i < Math.Abs(xShift); i++)
                {
                    if (0 < xShift)
                    {
                        ShiftLoadedChunksLeft();
                    }
                    else
                    {
                        ShiftLoadedChunksRight();
                    }
                }
                for (int i = 0; i < Math.Abs(zShift); i++)
                {
                    if (0 > zShift)
                    {
                        ShiftLoadedChunksUp();
                    }
                    else
                    {
                        ShiftLoadedChunksDown();
                    }
                }
                lock (_loadedChunkCenterlock)
                {
                    _loadedChunkCenter = GetChunkIndex(FocusPosition);
                }
            }//shifts chunks to align with new center
            public static void LoadChunks(Vector3 FocusPosition)
            {
                ShiftChunks(FocusPosition);
                General.IntDoubleArrayVisualizer.VisualizeToFile(_LoadedChunks);
                Chunk?[,] loadedChunks;
                int chunksLoadedFromArchive = 0;
                lock (_LoadedChunks)
                {
                    loadedChunks = _LoadedChunks;
                }
                lock (_IndecesToLoad)
                {
                    foreach (IntDouble chunkIndex in _IndecesToLoad)
                    {
                        if (loadedChunks[chunkIndex.X, chunkIndex.Z] == null)
                        {
                            IntDouble worldIndex = ChunksIndexToWorldIndex(chunkIndex);
                            if (Chunk.PullChunkFromArchive(worldIndex, out loadedChunks[chunkIndex.X, chunkIndex.Z]))
                            {
                                chunksLoadedFromArchive++;
                            }
                            else
                            {
                                loadedChunks[chunkIndex.X, chunkIndex.Z] = new Chunk(worldIndex, true);
                            }
                        }
                    }
                    _IndecesToLoad.Clear();
                }
                lock (_LoadedChunks)
                {
                    _LoadedChunks = loadedChunks;
                }
                List<Chunk> chunksToArchive = new();
                lock (_ChunksToArchive)
                {
                    foreach (Chunk chunk in _ChunksToArchive)
                    {
                        chunksToArchive.Add(chunk);
                    }
                    _ChunksToArchive.Clear();
                }
                if (chunksToArchive.Count > 0)
                {
                    Game1.Log($"Archiving {chunksToArchive.Count} chunks");
                }
                if (chunksLoadedFromArchive > 0)
                {
                    Game1.Log($"Loaded {chunksLoadedFromArchive} chunks from archive");
                }

                foreach (Chunk chunk in chunksToArchive)
                {
                    if (chunk != null)
                    {
                        chunk.ArchiveChunk();
                    }
                }
            }
            /*
            public static bool GetChunkFromWorldPos(IntDouble worldPos, out Chunk outputChunk)
            {
                // Calculate chunk index with proper negative handling
                IntDouble chunkIndex = WorldIndexToLoadedChunksIndex(GetWorldChunkIndex(worldPos));
                // First check if the indices are valid
                if (chunkIndex.X < 0 || chunkIndex.X >= _loadedChunksSize ||
                    chunkIndex.Z < 0 || chunkIndex.Z >= _loadedChunksSize)
                {
                    outputChunk = null;
                    return false;
                }

                lock (_LoadedChunks)
                {
                    outputChunk = _LoadedChunks[chunkIndex.X, chunkIndex.Z];
                }
                return outputChunk != null;

                /* // If the chunk is not loaded, try to pull it from the archive, however may cause performance issues
                if (Chunk.PullChunkFromArchive(chunkIndex, ref outputChunk))
                {
                    return true;
                }
                else
                {
                    outputChunk = null;
                    return false; // Chunk pull failed
                }
                
            }
            */
            public static ref Chunk GetChunkRefFromChunksIndex(IntDouble chunkIndex)
            {
                // Check if the indices are valid
                if (chunkIndex.X < 0 || chunkIndex.X >= _loadedChunksSize ||
                    chunkIndex.Z < 0 || chunkIndex.Z >= _loadedChunksSize)
                {
                    Exception ChunkOutsideOfLoadedChunks;
                }

                lock (_LoadedChunks)
                {
                    return ref _LoadedChunks[chunkIndex.X, chunkIndex.Z];
                }
            }
            public static bool GetChunkCopyFromChunksIndex(IntDouble chunkIndex, out Chunk outputChunk)
            {
                // Check if the indices are valid
                if (chunkIndex.X < 0 || chunkIndex.X >= _loadedChunksSize ||
                    chunkIndex.Z < 0 || chunkIndex.Z >= _loadedChunksSize)
                {
                    outputChunk = null;
                    return false;
                }

                lock (_LoadedChunks)
                {
                    outputChunk = _LoadedChunks[chunkIndex.X, chunkIndex.Z];
                }
                return outputChunk != null;
            }
            public static IntDouble WorldIndexToLoadedChunksArrayIndex(IntDouble worldIndex)
            {
                lock (_loadedChunkCenterlock)
                {
                    return worldIndex - _loadedChunkCenter + new IntDouble(renderDistance, renderDistance);
                }
            }
            public static IntDouble ChunksIndexToWorldIndex(IntDouble LoadedChunksIndex)
            {
                lock (_loadedChunkCenterlock)
                {
                    return LoadedChunksIndex - new IntDouble(renderDistance, renderDistance) + _loadedChunkCenter;
                }
            }
            /*
            public static bool GetTileFromWorldPos(Vector3 worldPos, ref Tile outputTile)
            {
                if (GetChunkFromWorldPos(new(worldPos), out Chunk chunk))
                {
                    outputTile = ref chunk.getTile(worldPos);
                    return true;
                }
                return false; // Return an empty tile if chunk retrieval fails

            }
            */
            public static ref Tile GetTileRefFromIndex(IntTriple trueTileIndex)
            {
                // Convert tile index to chunk index (divide by chunk size)
                IntDouble worldIndex = new IntDouble(trueTileIndex) / Chunk._chunkSize;
                IntDouble ChunksIndex = WorldIndexToLoadedChunksArrayIndex(worldIndex);

                return ref GetChunkRefFromChunksIndex(ChunksIndex).getTileFromWorldIndex(new IntTriple(
                    trueTileIndex.X - (worldIndex.X * Chunk._chunkSize),
                    trueTileIndex.Y,
                    trueTileIndex.Z - (worldIndex.Z * Chunk._chunkSize)
                    ));
            }
            public static bool GetTileCopyFromIndex(IntTriple trueTileIndex, out Tile outputTile)
            {
                //if (trueTileIndex.Y > 256 || trueTileIndex.Y < 0) { outputTile = new Tile(); return false; } // bounds checking

                // Convert tile index to chunk index (divide by chunk size)
                IntDouble worldIndex = new IntDouble(trueTileIndex) / Chunk._chunkSize;
                IntDouble ChunksIndex = WorldIndexToLoadedChunksArrayIndex(worldIndex);
                if (GetChunkCopyFromChunksIndex(ChunksIndex, out Chunk chunk))
                {
                    outputTile = chunk.getTileFromWorldIndex(new IntTriple(
                        trueTileIndex.X - (worldIndex.X * Chunk._chunkSize),
                        trueTileIndex.Y,
                        trueTileIndex.Z - (worldIndex.Z * Chunk._chunkSize)
                        ));
                    return true; // succesfully taken tile
                }
                outputTile = new Tile(); // Return an empty tile if chunk retrieval fails
                return false;
            }
            public static IntDouble GetChunkIndex(Vector3 worldPos)
            {
                return new IntDouble(
                    (int)(worldPos.X / Chunk._trueChunkSize),
                    (int)(worldPos.Z / Chunk._trueChunkSize));
            }
            public static IntDouble GetWorldChunkIndex(IntDouble worldPos)
            {
                return worldPos / Chunk._trueChunkSize;
            }
        }
    }
}
