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
    namespace tilemap_system
    {
        internal class World
        {
            private static Dictionary<IntDouble, Chunk> _chunks = new Dictionary<IntDouble, Chunk>();

            public static Chunk GetChunkFromWorldPos(IntDouble pos)
            {
                // Calculate chunk index with proper negative handling
                IntDouble currentChunkIndex = new IntDouble(
                    (int)Math.Floor(pos.X / (float)Chunk._trueChunkSize),
                    (int)Math.Floor(pos.Z / (float)Chunk._trueChunkSize)
                );

                // Try to get existing chunk
                if (_chunks.TryGetValue(currentChunkIndex, out Chunk currentChunk))
                {
                    return currentChunk;
                }

                // Create and add new chunk if it doesn't exist
                var newChunk = new Chunk(currentChunkIndex);
                _chunks[currentChunkIndex] = newChunk; // Direct assignment is more efficient than TryAdd
                return newChunk;
            }

            public static ref Tile GetTileFromWorldPos(Vector3 pos)
            {
            

                Chunk chunk = GetChunkFromWorldPos(new(pos));
                return ref chunk.getTile(pos);
            }

            public static ref Tile GetTileFromIndex(IntTriple index)
            {
                // Convert tile index to chunk index (divide by chunk size)
                IntDouble chunkIndex = new IntDouble(
                    (int)Math.Floor(index.X / (float)Chunk._chunkSize),
                    (int)Math.Floor(index.Z / (float)Chunk._chunkSize));

                Chunk chunk = GetChunkFromWorldPos(chunkIndex);
                return ref chunk.getTileFromWorldIndex(new IntTriple(
                    index.X - (chunkIndex.X * Chunk._chunkSize),
                    index.Y,
                    index.Z - (chunkIndex.Z * Chunk._chunkSize)
                ));
            }

            // Helper method to get chunk index from world position
            public static IntDouble GetChunkIndex(Vector3 worldPos)
            {
                return new IntDouble(
                    (int)Math.Floor(worldPos.X / Chunk._trueChunkSize),
                    (int)Math.Floor(worldPos.Z / Chunk._trueChunkSize));
            }
        }
    }
}
