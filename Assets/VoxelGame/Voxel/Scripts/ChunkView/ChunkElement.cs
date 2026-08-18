using VoxelGame.Voxel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class ChunkElement
    {
        public bool IsInited { get; private set; }
        public bool HasCollider { get; private set; }

        public ChunkElementMeshData MeshData { get; private set; }
        private GameObject gameObject;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        public ChunkElement(bool hasCollider)
        {
            this.HasCollider = hasCollider;
        }

        public void Init(Vector2Int coords, Vector3 postition, Material material, Transform parent, string prefix, string customLayer = null)
        {
            if (IsInited)
                return;

            gameObject = new GameObject($"{prefix} <{coords.x},{coords.y}>");
            gameObject.transform.position = postition;
            gameObject.transform.SetParent(parent);
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            MeshData = new ChunkElementMeshData();
            meshFilter.sharedMesh = MeshData.mesh;

            if (customLayer != null)
                gameObject.layer = LayerMask.NameToLayer(customLayer);

            IsInited = true;
        }

        public void Delete()
        {
            if (!IsInited)
                return;

            MeshData.mesh.Clear();

            MeshData = null;

            if (meshCollider != null)
                GameObject.Destroy(meshCollider.sharedMesh);
            GameObject.Destroy(meshFilter.sharedMesh);
            GameObject.Destroy(meshRenderer.sharedMaterial);
            GameObject.Destroy(gameObject);

            IsInited = false;
        }

        public void AddFadeAnimation()
        {
            if (IsInited)
                gameObject.AddComponent<ChunkFade>();
        }

        public void UpdateMesh()
        {
            if (!IsInited)
                return;

            if (MeshData == null)
                return;

            if (MeshData.mesh == null)
                return;

            MeshData.mesh.Clear();

            if (MeshData.verts.Count != MeshData.vertexColors.Count)
                return;

            MeshData.mesh.SetVertices(MeshData.verts);
            MeshData.mesh.SetTriangles(MeshData.tris, 0);
            MeshData.mesh.SetUVs(0, MeshData.uvs);
            MeshData.mesh.SetColors(MeshData.vertexColors);

            MeshData.mesh.RecalculateNormals();
            MeshData.mesh.Optimize();

            MeshData.Clear();

            if (HasCollider)
                CreateModelCollider();
        }

        private void CreateModelCollider()
        {
            if (gameObject == null)
                return;

            if (meshCollider == null)
            {
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }

            meshCollider.sharedMesh = MeshData.mesh.triangles.Length > 0 ? MeshData.mesh : null;
        }
    }
}