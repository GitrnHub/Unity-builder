using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class ChunkView : IChunkView
    {
        private ChunkElement chunkElementMain;
        private ChunkElement chunkElementDecor;
        private ChunkElement chunkElementTransp;
        private ChunkElement currentChunkElement;
        private ChunkData chunkData;

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        private readonly ChunkSystem chunkSystem;
        private readonly VoxelSettingsSO voxelSettings;

        public ChunkView(ChunkSystem chunkSystem, VoxelSettingsSO voxelSettings)
        {
            this.chunkSystem = chunkSystem;
            this.voxelSettings = voxelSettings;

            chunkElementMain = new ChunkElement(hasCollider: true);
            chunkElementDecor = new ChunkElement(hasCollider: false);
            chunkElementTransp = new ChunkElement(hasCollider: false);

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        public void Update(ChunkData chunkData, Action<Vector2Int> onUpdateCallback)
        {
            if (chunkData == null)
            {
                this.chunkData = null;
                chunkElementMain.Delete();
                chunkElementDecor.Delete();
                chunkElementTransp.Delete();
                return;
            }
            else
            {
                this.chunkData = chunkData;
            }

            if (!chunkElementMain.IsInited)
            {
                chunkElementMain.Init(
                    chunkData.Coords,
                    chunkData.Position,
                    voxelSettings.ChunksMaterial,
                    chunkSystem.transform,
                    "chunk main",
                    "Voxel");

                chunkElementMain.AddFadeAnimation();
            }

            if (!chunkElementDecor.IsInited)
            {
                chunkElementDecor.Init(
                    chunkData.Coords,
                    chunkData.Position,
                    voxelSettings.ChunksMaterial,
                    chunkSystem.transform,
                    "chunk decor",
                    "VoxelDecor");

                chunkElementDecor.AddFadeAnimation();
            }

            if (!chunkElementTransp.IsInited)
            {
                chunkElementTransp.Init(
                    chunkData.Coords,
                    chunkData.Position,
                    voxelSettings.ChunksTranspMaterial,
                    chunkSystem.transform,
                    "chunk transp");

                chunkElementTransp.AddFadeAnimation();
            }

            Task.Run(() => Generate(), cancellationToken).GetAwaiter().OnCompleted(() =>
            {
                if (chunkData != null)
                {
                    chunkElementMain?.UpdateMesh();
                    chunkElementDecor?.UpdateMesh();
                    chunkElementTransp?.UpdateMesh();

                    onUpdateCallback(this.chunkData.Coords);
                }
            });
        }

        public void Delete()
        {
            CancelTask();

            chunkData = null;
            currentChunkElement = null;

            chunkElementMain?.Delete();
            chunkElementMain = null;

            chunkElementDecor?.Delete();
            chunkElementDecor = null;

            chunkElementTransp?.Delete();
            chunkElementTransp = null;
        }

        private void CancelTask()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }

        private void Generate()
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                    for (int y = 0; y < VoxelData.chunkHeight; y++)
                        if (chunkData.map[x, y, z] > 0)
                        {
                            Voxel voxel = Voxels.voxels[chunkData.map[x, y, z]];

                            currentChunkElement = voxel.isTransp ? chunkElementTransp : voxel.isDecor ? chunkElementDecor : chunkElementMain;

                            if (voxel.isShapeX)
                            {
                                CreateVoxelShapeX(new Vector3Int(x, y, z), chunkData.map[x, y, z]);
                            }
                            else
                            {
                                CreateVoxel(new Vector3Int(x, y, z), chunkData.map[x, y, z]);
                            }
                        }

            CreateVertexColors(chunkElementMain.MeshData);
            CreateVertexColors(chunkElementDecor.MeshData);
            CreateVertexColors(chunkElementTransp.MeshData);
        }

        private void CreateVoxel(Vector3Int pos, byte voxel)
        {
            for (int p = 0; p < 6; p++)
            {
                if (pos.y == 0 && p == 3)
                    continue;

                if (VoxelIsEmpty(pos + VoxelData.faceChecksInt[p]))
                {
                    if (Voxels.voxels[voxel].isSolidForSame && GetVoxel(pos + VoxelData.faceChecksInt[p]) == voxel)
                        continue;

                    currentChunkElement.MeshData.AddFace(pos, voxel, p);
                }
            }
        }

        private void CreateVoxelShapeX(Vector3Int pos, byte voxel)
        {
            for (int p = 0; p < 6; p++)
            {
                if (!(p == 2 || p == 3))
                {
                    currentChunkElement.MeshData.AddFaceX(pos, voxel, p);
                }
            }
        }

        private bool VoxelIsEmpty(Vector3Int pos)
        {
            if (!VoxelUtils.IsVoxelInChunk(pos))
                return Voxels.voxels[chunkSystem.GetVoxel(pos + chunkData.Position)].isEmpty;

            return Voxels.voxels[chunkData.map[pos.x, pos.y, pos.z]].isEmpty;
        }

        private byte GetVoxel(Vector3Int pos)
        {
            if (!VoxelUtils.IsVoxelInChunk(pos))
                return chunkSystem.GetVoxel(pos + chunkData.Position);

            return chunkData.map[pos.x, pos.y, pos.z];
        }

        #region Lighting
        public void CreateVertexColors(ChunkElementMeshData meshData)
        {
            int vertCount = meshData.verts.Count;

            for (int v = 0; v < vertCount; v++)
            {
                byte sun = GetSoftLight(meshData.verts[v]);
                byte ao = (byte)(Mathf.Clamp(CalcNearSolidBlocks(meshData.verts[v], meshData.voxelsP[v]), 0, 3) * (4 + sun * 0.3f));

                sun = (byte)Mathf.Clamp(32 + sun * 0.88f, byte.MinValue, byte.MaxValue);

                byte light = (byte)(Mathf.Clamp(sun - ao, byte.MinValue, byte.MaxValue));

                meshData.vertexColors.Add(new Color32(light, light, light, 255));
            }
        }

        private int CalcNearSolidBlocks(Vector3 vert, int p)
        {
            int count = 0;

            switch (p)
            {
                case 0:
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, 0.5f, -0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, 0.5f, -0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, -0.5f, -0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, -0.5f, -0.5f))))
                        count++;
                    break;
                case 1:
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, 0.5f, 0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, 0.5f, 0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, -0.5f, 0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, -0.5f, 0.5f))))
                        count++;
                    break;
                case 2:
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, 0.5f, 0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, 0.5f, 0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, 0.5f, -0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, 0.5f, -0.5f))))
                        count++;
                    break;
                case 3:
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, -0.5f, 0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, -0.5f, 0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, -0.5f, -0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, -0.5f, -0.5f))))
                        count++;
                    break;
                case 4:
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, 0.5f, 0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, 0.5f, -0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, -0.5f, 0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(-0.5f, -0.5f, -0.5f))))
                        count++;
                    break;
                case 5:
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, 0.5f, 0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, 0.5f, -0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, -0.5f, 0.5f))))
                        count++;
                    if (!VoxelIsEmpty(Vector3Int.FloorToInt(vert + new Vector3(0.5f, -0.5f, -0.5f))))
                        count++;
                    break;
                default: break;
            }

            return count;
        }

        private byte GetSoftLight(Vector3 vert)
        {
            int total = 0;
            int count = 0;

            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3Int p = Vector3Int.FloorToInt(vert + new Vector3(x, y, z) - Vector3.one * 0.5f);
                        if (VoxelIsEmpty(p))
                        {
                            if (VoxelUtils.IsVoxelInChunk(p))
                            {
                                total += chunkData.light[p.x, p.y, p.z];
                            }
                            else
                            {
                                total += (p.y >= 0 && p.y < VoxelData.chunkHeight) ? chunkSystem.GetLight(p + chunkData.Position) : byte.MaxValue;
                            }

                            count++;
                        }
                    }
            return (byte)(Mathf.Clamp(total / count, 0, 255));
        }
        #endregion
    }
}
