using System.Collections.Generic;
using UnityEngine;
using Zenject;

using UnityEngine.Events;

namespace VoxelGame.Voxel
{
    public class ChunkSystem : MonoBehaviour
    {
        public UnityEvent<Vector3Int, byte> OnVoxelDestroyed = new();
        public Dictionary<Vector2Int, Chunk> Chunks {get; set;}
        private Transform cam;
        private VoxelSettingsSO voxelSettings;
        private ISaveLoadChunk saveLoad;
        private Chunk.Factory chunkFactory;
        private IChunkControl chunkControl;

        [Inject]
        private void Contstruct(
            VoxelSettingsSO voxelSettings,
            ISaveLoadChunk saveLoad,
            Chunk.Factory chunkFactory,
            IChunkControl chunkControl)
        {
            this.voxelSettings = voxelSettings;
            this.saveLoad = saveLoad;
            this.chunkFactory = chunkFactory;
            this.chunkControl = chunkControl;
        }

        private void Start()
        {
            if (cam == null)
            {
                cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
            }

            Chunks = new Dictionary<Vector2Int, Chunk>();
            chunkControl = new ChunkControl(this);
        }
        
        private void Update()
        {
            AutoSaveControl.Update(saveLoad, Chunks);
            chunkControl.UpdateChunks(cam, voxelSettings.RenderDistance, voxelSettings.GenerateOnlyInFrustum);
        }

        public void AddChunk(Vector2Int coords)
        {
            var chunk = chunkFactory.Create(coords);
            Chunks.Add(coords, chunk);
        }

        public Chunk GetChunk(Vector2Int coords)
        {
            if (!Chunks.ContainsKey(coords))
                return null;

            if (!Chunks[coords].IsLoaded)
                return null;

            return Chunks[coords];
        }

        #region Chunk voxel operations
        public byte GetVoxel(Vector3Int worldPos)
        {
            Vector2Int coords = VoxelUtils.GetChunkCoords(worldPos);

            if (Chunks.TryGetValue(coords, out var chunk))
            {
                if (chunk != null)
                {
                    Vector3Int localPos = VoxelUtils.GetLocalPos(worldPos, coords);
                    if (VoxelUtils.IsVoxelInChunk(localPos))
                    {
                        return chunk.GetVoxel(localPos);
                    }
                }
            }

            return 0;
        }

        public void SetVoxel(Vector3Int worldPos, byte voxel, bool updateNearChunks, bool setSaveFlag, bool destroyDecor = false)
        {
            Vector2Int coords = VoxelUtils.GetChunkCoords(worldPos);

            if (Chunks.TryGetValue(coords, out var chunk))
            {
                if (chunk == null)
                    return;

                if (chunkControl.ChunkIsUpdating(coords))
                    return;

                Vector3Int localPos = VoxelUtils.GetLocalPos(worldPos, coords);

                byte changedVoxel = GetVoxel(worldPos);

                if (chunk.SetVoxel(localPos, voxel, true))
                {
                    if (changedVoxel != Voxels.AIR)    
                        OnVoxelDestroyed.Invoke(worldPos, changedVoxel);

                    if (destroyDecor && voxel == Voxels.AIR)
                    {
                        changedVoxel = GetVoxel(worldPos + Vector3Int.up);

                        if (Voxels.voxels[changedVoxel].isDecor)
                        {
                            chunk.SetVoxel(localPos + Vector3Int.up, voxel);

                            OnVoxelDestroyed.Invoke(worldPos + Vector3Int.up, changedVoxel);
                        }
                    }

                    chunkControl.AddToUpdateQueue(coords);

                    if (updateNearChunks)
                    {
                        if (localPos.x == 0)
                            chunkControl.AddToUpdateQueue(coords - Vector2Int.right);
                        else if (localPos.x == VoxelData.chunkWidth - 1)
                            chunkControl.AddToUpdateQueue(coords + Vector2Int.right);

                        if (localPos.z == 0)
                            chunkControl.AddToUpdateQueue(coords - Vector2Int.up);
                        else if (localPos.z == VoxelData.chunkWidth - 1)
                            chunkControl.AddToUpdateQueue(coords + Vector2Int.up);
                    }
                }
            }
        }

        public void SetVoxelsRadius(Vector3Int pos, byte voxel, int radius)
        {
            for (int x = pos.x - radius; x <= pos.x + radius; x++)
                for (int y = pos.y - radius; y <= pos.y + radius; y++)
                    for (int z = pos.z - radius; z <= pos.z + radius; z++)
                    {
                        Vector3Int p = new Vector3Int(x, y, z);

                        if (Vector3Int.Distance(pos, p) <= radius)
                        {
                            SetVoxel(p, voxel, true, true, destroyDecor: true);

                        }
                    }

            chunkControl.SortUpdateQueue(cam);
        }

        public byte GetLight(Vector3Int worldPos)
        {
            Vector2Int coords = VoxelUtils.GetChunkCoords(worldPos);

            if (Chunks.TryGetValue(coords, out var chunk))
            {
                if (chunk != null)
                {
                    Vector3Int localPos = VoxelUtils.GetLocalPos(worldPos, coords);
                    if (VoxelUtils.IsVoxelInChunk(localPos))
                    {
                        return chunk.GetLight(localPos);
                    }
                }
            }

            return byte.MaxValue;
        }
        #endregion
    }
}