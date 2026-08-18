using UnityEngine;

namespace VoxelGame.Voxel
{
    public interface ISaveLoadChunk
    {
        public void AddToSaveQueue(SavedChunkData data);
        public void Save(int maxSavedChunks);
        public SavedChunkData Load(Vector2Int coords);
        public void DeleteAll();
    }
}