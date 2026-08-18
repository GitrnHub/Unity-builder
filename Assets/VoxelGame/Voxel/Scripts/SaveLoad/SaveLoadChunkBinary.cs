using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using OdinSerializer;
using VoxelGame.Utilities;

namespace VoxelGame.Voxel
{
    public class SaveLoadChunkBinary : SaveLoadChunk
    {
        public override SavedChunkData Load(Vector2Int coords)
        {
            string destination = GetDestination(coords);

            if (File.Exists(destination))
            {
                byte[] bytes = ByteArrayCompressor.Decompress(File.ReadAllBytes(destination));
                SavedChunkData loadedData = SerializationUtility.DeserializeValue<SavedChunkData>(bytes, DataFormat.Binary);
                return loadedData;
            }

            return null;
        }

        protected override void SaveChunk(SavedChunkData data)
        {
            string destination = GetDestination(new Vector2Int(data.x, data.z));
            byte[] bytes = ByteArrayCompressor.Compress(SerializationUtility.SerializeValue(data, DataFormat.Binary));
            File.WriteAllBytes(destination, bytes);
        }
    }
}