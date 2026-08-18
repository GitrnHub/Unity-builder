using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace VoxelGame.Voxel
{
    public class ChunkControl : IChunkControl
    {
        private readonly float addChunksDelay = 2f;
        private readonly int maxLoadedChunksPerUpdate = 2;
        private readonly int maxUpdatedChunksPerUpdate = 3;
        private float timerAddChunks = 3f;

        private Queue<Vector2Int> loadQueue;
        private Queue<Vector2Int> updateQueue;
        private List<Vector2Int> updatingChunks;
        private readonly ChunkSystem chunkSystem;

        public ChunkControl(ChunkSystem chunkSystem)
        {
            this.chunkSystem = chunkSystem;

            loadQueue = new Queue<Vector2Int>();
            updateQueue = new Queue<Vector2Int>();
            updatingChunks = new List<Vector2Int>();
        }

        public void UpdateChunks(Transform cam, int renderDistance, bool inFrustum)
        {
            AddChunkControl(cam, renderDistance, inFrustum);

            if (Time.frameCount % 6 == 0)
            {
                LoadChunkControl(cam);
                SortUpdateQueue(cam);
            }
            else
            {
                UpdateChunkControl();
            }
        }

        public void AddToUpdateQueue(Vector2Int coords)
        {
            if (updatingChunks.Contains(coords))
                return;

            if (!updateQueue.Contains(coords))
                updateQueue.Enqueue(coords);
        }

        public bool ChunkIsUpdating(Vector2Int coords)
        {
            return updatingChunks.Contains(coords);
        }    

        public void SortUpdateQueue(Transform cam)
        {
            Vector2Int playerChunk = VoxelUtils.GetChunkCoords(Vector3Int.FloorToInt(cam.position));
            updateQueue = new Queue<Vector2Int>(updateQueue.OrderBy(c => (c - playerChunk).sqrMagnitude));
        }
            
        private void AddChunkControl(Transform cam, int chunkDistance, bool inFrustumOnly)
        {
            timerAddChunks += Time.deltaTime;
            if (timerAddChunks > addChunksDelay)
            {
                timerAddChunks = 0f;
                Vector2Int playerChunk = VoxelUtils.GetChunkCoords(Vector3Int.FloorToInt(cam.position));
                AddChunks(playerChunk, chunkDistance, inFrustumOnly, cam.forward);
                DeleteChunks(chunkDistance + 2, playerChunk);
            }
        }

        private void AddChunks(Vector2Int center, int size, bool addInFrustumOnly, Vector3 frustumDir)
        {
            for (int x = -size; x <= size; x++)
                for (int z = -size; z <= size; z++)
                {
                    Vector2Int offset = new Vector2Int(x, z);

                    float dot = Vector2.Dot(
                        new Vector2(frustumDir.x, frustumDir.z),
                        ((Vector2)offset).normalized
                        );

                    float dotSide = Vector2.Dot(
                        new Vector2(frustumDir.z, frustumDir.x),
                        ((Vector2)offset).normalized
                        );

                    if (addInFrustumOnly)
                    {
                        if ((offset.magnitude <= size / 4 + 1) || (dot > -0.25f && offset.magnitude <= size && Mathf.Abs(dotSide) < Mathf.Abs(dot) + 0.5f))
                        {
                            AddChunk(center + offset);
                        }
                    }
                    else
                    {
                        if (offset.magnitude <= size)
                        {
                            AddChunk(center + offset);
                        }
                    }
                }
        }

        private void AddChunk(Vector2Int coords)
        {
            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                {
                    Vector2Int currentCoords = new Vector2Int(x, z) + coords;

                    if (!chunkSystem.Chunks.ContainsKey(currentCoords))
                    {
                        chunkSystem.AddChunk(currentCoords);

                        if (loadQueue.Contains(currentCoords))
                            continue;

                        if (updatingChunks.Contains(currentCoords))
                            continue;

                        loadQueue.Enqueue(currentCoords);
                    }
                }
        }

        private void LoadChunkControl(Transform cam)
        {
            Vector2Int playerChunk = VoxelUtils.GetChunkCoords(Vector3Int.FloorToInt(cam.position));
            loadQueue = new Queue<Vector2Int>(loadQueue.OrderBy(c => (c - playerChunk).sqrMagnitude));

            int counter = 0;
            while (loadQueue.Count > 0 && counter < maxLoadedChunksPerUpdate)
            {
                var coords = loadQueue.Dequeue();
                if (chunkSystem.Chunks.TryGetValue(coords, out var chunk))
                {
                    if (chunk == null)
                        continue;

                    if (chunk.IsLoaded)
                        continue;

                    chunk.Load((c) => AddToUpdateQueue(c));

                    counter++;
                }
            }
        }

        private void DeleteChunks(float chunkDistance, Vector2Int playerChunk)
        {
            List<Vector2Int> chunksToDelete = new List<Vector2Int>();

            foreach (var coords in chunkSystem.Chunks.Keys)
            {
                if (updatingChunks.Contains(coords))
                    continue;

                if (Vector2Int.Distance(playerChunk, coords) > chunkDistance)
                    chunksToDelete.Add(coords);
            }

            foreach (var coords in chunksToDelete)
            {
                chunkSystem.Chunks[coords].Delete();
                chunkSystem.Chunks[coords] = null;
                chunkSystem.Chunks.Remove(coords);
            }
        }

        private void UpdateChunkControl()
        {
            List<Vector2Int> chunksList = new List<Vector2Int>();
            List<Vector2Int> chunksReturnedList = new List<Vector2Int>();

            int counter = 0;
            int tries = 32;
            while (updateQueue.Count > 0 && counter < maxUpdatedChunksPerUpdate && tries > 0)
            {
                var coords = updateQueue.Dequeue();
                if (chunkSystem.Chunks.TryGetValue(coords, out var chunk))
                {
                    if (chunk == null)
                        continue;

                    if (chunk.IsLoaded &&
                        CanUpdateChunk(coords) &&
                        !chunksList.Contains(coords) &&
                        !updatingChunks.Contains(coords))
                    {
                        chunksList.Add(coords);
                        counter++;
                    }
                    else
                    {
                        chunksReturnedList.Add(coords);
                    }

                    tries--;
                }
            }

            foreach (var coords in chunksList)
            {
                chunkSystem.Chunks[coords].CalculateLight();
            }

            foreach (var coords in chunksList)
            {
                updatingChunks.Add(coords);
                chunkSystem.Chunks[coords].Update((c) => RemoveFromUpdatingList(c));
            }

            foreach (var coords in chunksReturnedList)
            {
                updateQueue.Enqueue(coords);
            }
        }

        private bool CanUpdateChunk(Vector2Int coords)
        {
            for (int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                {
                    Vector2Int c = new Vector2Int(x, z) + coords;

                    if (!chunkSystem.Chunks.ContainsKey(c))
                        return false;

                    if (!chunkSystem.Chunks[c].IsLoaded)
                        return false;
                }

            return true;
        }

        private void RemoveFromUpdatingList(Vector2Int coords)
        {
            if (updatingChunks.Contains(coords))
                updatingChunks.Remove(coords);
        }
    }
}
