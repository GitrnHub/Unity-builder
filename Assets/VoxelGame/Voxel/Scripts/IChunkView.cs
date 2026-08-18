using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public interface IChunkView
    {
        public void Update(ChunkData chunkModel, Action<Vector2Int> onUpdateCallback);
        public void Delete();
    }
}
