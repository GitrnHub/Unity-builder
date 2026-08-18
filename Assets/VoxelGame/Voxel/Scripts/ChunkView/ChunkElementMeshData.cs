using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mesh;

namespace VoxelGame.Voxel
{
    public class ChunkElementMeshData
    {
        public int vertIndex = 0;

        public List<Vector3> verts;
        public List<int> tris;
        public List<Vector2> uvs;
        public List<Color32> vertexColors;
        public List<Vector3Int> voxels;
        public List<int> voxelsP;

        public Mesh mesh;

        public ChunkElementMeshData()
        {
            mesh = new Mesh();

            verts = new List<Vector3>();
            tris = new List<int>();
            uvs = new List<Vector2>();
            vertexColors = new List<Color32>();
            voxels = new List<Vector3Int>();
            voxelsP = new List<int>();
        }

        public void Clear()
        {
            vertIndex = 0;

            verts.Clear();
            tris.Clear();
            uvs.Clear();
            vertexColors.Clear();
            voxels.Clear();
            voxelsP.Clear();
        }

        public void AddFace(Vector3Int pos, byte blockId, int p)
        {
            voxels.Add(pos);
            voxels.Add(pos);
            voxels.Add(pos);
            voxels.Add(pos);

            voxelsP.Add(p);
            voxelsP.Add(p);
            voxelsP.Add(p);
            voxelsP.Add(p);

            verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
            verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
            verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
            verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

            if (p == 2)
                AddTexture(Voxels.voxels[blockId].textureId);
            else if (p == 3)
                AddTexture(Voxels.voxels[blockId].textureIdBottom);
            else
                AddTexture(Voxels.voxels[blockId].textureIdSide);

            tris.Add(vertIndex);
            tris.Add(vertIndex + 1);
            tris.Add(vertIndex + 2);
            tris.Add(vertIndex + 2);
            tris.Add(vertIndex + 1);
            tris.Add(vertIndex + 3);
            vertIndex += 4;
        }

        public void AddFaceX(Vector3Int pos, byte blockId, int p)
        {
            voxels.Add(pos);
            voxels.Add(pos);
            voxels.Add(pos);
            voxels.Add(pos);

            voxelsP.Add(p);
            voxelsP.Add(p);
            voxelsP.Add(p);
            voxelsP.Add(p);

            if (p == 0)
            {
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[0, 0]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[0, 1]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[5, 2]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[5, 3]]);
            }
            else if (p == 1)
            {
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[1, 0]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[1, 1]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[4, 2]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[4, 3]]);

            }
            else if (p == 4)
            {
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[4, 0]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[4, 1]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[0, 2]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[0, 3]]);
            }
            else
            {
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[5, 0]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[5, 1]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[1, 2]]);
                verts.Add(pos + VoxelData.voxelVertsDecorGrass[VoxelData.voxelTris[1, 3]]);
            }

            AddTexture(Voxels.voxels[blockId].textureId);

            tris.Add(vertIndex);
            tris.Add(vertIndex + 1);
            tris.Add(vertIndex + 2);
            tris.Add(vertIndex + 2);
            tris.Add(vertIndex + 1);
            tris.Add(vertIndex + 3);

            vertIndex += 4;
        }

        private void AddTexture(int textureId)
        {
            float y = textureId / VoxelData.textureAtlasSize;
            float x = textureId - (y * VoxelData.textureAtlasSize);

            x *= VoxelData.textureAtlasElementSize;
            y *= VoxelData.textureAtlasElementSize;

            y = 1f - y - VoxelData.textureAtlasElementSize;

            uvs.Add(new Vector2(x, y));
            uvs.Add(new Vector2(x, y + VoxelData.textureAtlasElementSize));
            uvs.Add(new Vector2(x + VoxelData.textureAtlasElementSize, y));
            uvs.Add(new Vector2(x + VoxelData.textureAtlasElementSize, y + VoxelData.textureAtlasElementSize));
        }
    }
}