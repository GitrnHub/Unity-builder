using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public interface IGenerator
    {
        public void GenerateMap(Vector3Int pos, ref byte[,,] map);
        public void GenerateChunkStructures(Vector2Int coords, ref byte[,,] map);
    }
}
