using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class ChunkStructure
    {
        public Vector3Int position;
        public VoxelStructure structure;

        public static void CreateStructures(Vector2Int coords, byte[,,] map, List<ChunkStructure> structures)
        {
            Vector3Int position = VoxelUtils.GetChunkPosition(coords);

            foreach (var chunkStructure in structures)
            {
                Vector3Int pos = chunkStructure.position - position;

                foreach (var structureElement in chunkStructure.structure.elements)
                {
                    Vector3Int v = pos + structureElement.offset;

                    if (VoxelUtils.IsVoxelInChunk(v))
                    {
                        map[v.x, v.y, v.z] = structureElement.voxel;
                    }
                }
            }
        }
    }
}