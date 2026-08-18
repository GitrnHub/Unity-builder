using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace VoxelGame.Voxel
{
    public class Chunk
    {
        public bool IsLoaded { get; private set; }

        private bool saveFlag;

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        private ChunkData data;
        private IChunkView view;
        private readonly IGenerator generator;
        private readonly ISaveLoadChunk saveLoad;

        public Chunk(Vector2Int coords, IChunkView view, IGenerator generator, ISaveLoadChunk saveLoad)
        {
            this.view = view;
            this.data = new ChunkData(coords);
            this.generator = generator;
            this.saveLoad = saveLoad;

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        public void Load(Action<Vector2Int> onLoadCallback)
        {
            if (IsLoaded)
                return;

            if (data == null)
                return;

            SavedChunkData savedData = saveLoad.Load(data.Coords);

            if (savedData == null)
            {
                Task.Run(() =>
                {
                    GenerateMap();
                    CalculateLight();
                }, cancellationToken).GetAwaiter().OnCompleted(() =>
                {
                    IsLoaded = true;

                    if (data != null)
                        onLoadCallback(data.Coords);
                });
            }
            else
            {
                data.map = savedData.GetMap();

                if (data != null)
                {
                    CalculateLight();
                    IsLoaded = true;

                    onLoadCallback(data.Coords);
                } else
                {
                    IsLoaded = false;
                }
            }
        }

        public bool Save()
        {
            if (IsLoaded && saveFlag)
            {
                saveLoad.AddToSaveQueue(GetSavedData());
                saveFlag = false;
                return true;
            }

            return false;
        }

        public void Update(Action<Vector2Int> onUpdateCallback)
        {
            if (!IsLoaded)
                return;

            view.Update(data, onUpdateCallback);
        }

        public void Delete()
        {
            CancelTask();

            data = null;
            view.Delete();
            view = null;
        }

        private void CancelTask()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }

        public byte GetVoxel(Vector3Int pos)
        {
            return data.map[pos.x, pos.y, pos.z];
        }

        public bool SetVoxel(Vector3Int pos, byte voxel, bool setSaveFlag = false)
        {
            if (data == null)
                return false;

            if (setSaveFlag)
                saveFlag = true;

            if (VoxelUtils.IsVoxelInChunk(pos) && data.map[pos.x, pos.y, pos.z] != voxel)
            {
                data.map[pos.x, pos.y, pos.z] = voxel;
                return true;
            }

            return false;
        }

        public byte GetLight(Vector3Int pos)
        {
            return data.light[pos.x, pos.y, pos.z];
        }

        public SavedChunkData GetSavedData()
        {
            SavedChunkData savedData = new SavedChunkData();
            savedData.SetCoords(data.Coords);
            savedData.SetMap(data.map);
            return savedData;
        }

        public void CalculateLight()
        {
            if (data == null)
                return;

            for (int x = 0; x < VoxelData.chunkWidth; x++)
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    byte sunLight = byte.MaxValue;
                    byte lightFalloff = byte.MaxValue / 6;

                    for (int y = VoxelData.chunkHeight - 1; y >= 0; y--)
                    {
                        data.light[x, y, z] = sunLight;

                        if (y > 0 && data.map[x, y - 1, z] > 0 && sunLight > 0)
                        {
                            if (sunLight > lightFalloff)
                                sunLight -= lightFalloff;
                            else
                                sunLight = 0;
                        }
                    }
                }
        }

        private void GenerateMap()
        {
            if (IsLoaded)
                return;

            generator.GenerateMap(data.Position, ref data.map);
            generator.GenerateChunkStructures(data.Coords, ref data.map);
        }

        public class Factory : PlaceholderFactory<Vector2Int, Chunk>
        {
        }
    }
}