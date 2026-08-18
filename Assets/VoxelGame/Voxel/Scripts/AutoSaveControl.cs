using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public static class AutoSaveControl
    {
        private const int frameCountDelay = 120;
        private const int maxSavedChunks = 2;

        public static void Update(ISaveLoadChunk saveLoadChunk, Dictionary<Vector2Int, Chunk> chunks)
        {
            if (Time.frameCount % frameCountDelay != 0)
                return;

            int savedChunks = 0;

            foreach (var chunk in chunks.Values)
            {
                if (chunk != null && chunk.IsLoaded)
                {
                    if (chunk.Save())
                    {
                        savedChunks++;
                    }

                    if (savedChunks >= maxSavedChunks)
                        break;
                }
            }

            saveLoadChunk.Save(maxSavedChunks);
        }
    }
}