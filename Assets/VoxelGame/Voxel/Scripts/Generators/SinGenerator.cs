using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class SinGenerator : IGenerator
    {
        public void GenerateMap(Vector3Int position, ref byte[,,] map)
        {
            if (map == null)
                return;

            int lengthX = map.GetLength(0);
            int lengthY = map.GetLength(1);
            int lengthZ = map.GetLength(2);

            if (lengthX == 0 || lengthY == 0 || lengthZ == 0)
                return;

            float waveScale = 0.1f;
            float waveHeight = 0.05f;
            float waveYOffset = 0.25f;

            for (int x = 0; x < lengthX; x++)
                for (int z = 0; z < lengthZ; z++)
                    for (int y = 0; y < lengthY; y++)
                    {
                        float h = ((Mathf.Sin((position.x + x) * waveScale) + Mathf.Cos((position.z + z) * waveScale) + 2) * waveHeight + waveYOffset) * VoxelData.chunkHeight;

                        map[x, y, z] = y < h ? Voxels.STONE : Voxels.AIR;
                    }
        }

        public void GenerateChunkStructures(Vector2Int coords, ref byte[,,] map)
        {
            // no structures
        }
    }
}
