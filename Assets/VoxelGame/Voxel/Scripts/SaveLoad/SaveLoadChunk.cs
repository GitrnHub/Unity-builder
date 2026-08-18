using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace VoxelGame.Voxel
{
    public abstract class SaveLoadChunk : ISaveLoadChunk
    {
        private readonly string pathWorld = "world";
        private readonly string pathChunkPrefix = "chunk_";
        private readonly string pathEnd = ".dat";

        private readonly Queue<SavedChunkData> dataQueue = new Queue<SavedChunkData>();

        protected SaveLoadChunk()
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, pathWorld));
        }

        public void Save(int maxSavedChunks = 1)
        {
            for (int count = 0; count < maxSavedChunks && dataQueue.Count > 0; count++)
            {
                SavedChunkData data = dataQueue.Dequeue();
                SaveChunk(data);
            }
        }

        public void AddToSaveQueue(SavedChunkData data)
        {
            dataQueue.Enqueue(data);
        }

        public abstract SavedChunkData Load(Vector2Int coords);

        public void DeleteAll()
        {
            string path = Path.Combine(Application.persistentDataPath, pathWorld);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        protected abstract void SaveChunk(SavedChunkData data);

        protected string GetDestination(Vector2Int coords)
        {
            return Path.Combine(Application.persistentDataPath, pathWorld, pathChunkPrefix + coords.ToString() + pathEnd);
        }
    }
}