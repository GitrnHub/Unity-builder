    
using UnityEngine;

namespace VoxelGame.Voxel
{
    public interface IChunkControl
    {
        public void UpdateChunks(Transform cam, int renderDistance, bool inFrustum);
        public void AddToUpdateQueue(Vector2Int coords);
        public bool ChunkIsUpdating(Vector2Int coords);
        public void SortUpdateQueue(Transform cam);
    }
}