using System.IO;
using UnityEngine;
using OdinSerializer;
using VoxelGame.Utilities;

namespace VoxelGame.Voxel
{
    public class SaveLoadChunkJSON : SaveLoadChunk
    {
        public override SavedChunkData Load(Vector2Int coords)
        {
            string destination = GetDestination(coords);

            if (File.Exists(destination))
            {
                byte[] bytes = File.ReadAllBytes(destination);
                SavedChunkData loadedData = SerializationUtility.DeserializeValue<SavedChunkData>(bytes, DataFormat.JSON);
                loadedData.map = ByteArrayCompressor.Decompress(loadedData.map);
                return loadedData;
            }

            return null;
        }

        protected override void SaveChunk(SavedChunkData data)
        {
            string destination = GetDestination(new Vector2Int(data.x, data.z));
            data.map = ByteArrayCompressor.Compress(data.map);
            byte[] bytes = SerializationUtility.SerializeValue(data, DataFormat.JSON);
            File.WriteAllBytes(destination, bytes);
        }
    }
}