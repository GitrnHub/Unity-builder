using UnityEngine;

using VoxelGame.Core;

namespace VoxelGame.Voxel
{
    [CreateAssetMenu(fileName = "Voxel Settings", menuName = "Voxel/Voxel Settings Scriptable Object")]
    public class VoxelSettingsSO : ScriptableObject
    {
        [Header("Main")]
        [SerializeField] private bool generateOnlyInFrustum = true;
        [SerializeField] private int seed = -1;
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private int renderDistance = 10;
        [SerializeField] private float renderScale = 1f;
        [SerializeField] private bool debugGraphy = false;
        [SerializeField] private Material chunksMaterial;
        [SerializeField] private Material chunksTranspMaterial;

        [Header("Voxels and Structures")]
        [SerializeField] private VoxelStructureSO[] structuresSO;
        [SerializeField] private VoxelsPresetSO voxelPresetSO;
        [SerializeField] private VoxelStructures structures;

        public bool GenerateOnlyInFrustum => generateOnlyInFrustum;
        public int Seed
        {
            get
            {
                return seed;
            }
            set
            {
                if (seed != value)
                {
                    seed = value;
                    PlayerPrefs.SetInt("voxelSeed", Seed);
                    PlayerPrefs.Save();
                }
            }
        }
        public int TargetFrameRate
        {
            get
            {
                return targetFrameRate;
            }
            set
            {
                if (targetFrameRate != value)
                {
                    targetFrameRate = value;
                    GameRenderSettings.SetFrameRate(targetFrameRate);
                    PlayerPrefs.SetInt("voxelTargetFrameRate", targetFrameRate);
                    PlayerPrefs.Save();
                }
            }
        }
        public int RenderDistance
        {
            get
            {
                return renderDistance;
            }
            set
            {
                if (renderDistance != value && renderDistance > 3 && renderDistance < 32)
                {
                    renderDistance = value;
                    GameRenderSettings.SetFog((renderDistance - 2f) * VoxelData.chunkWidth, VoxelData.chunkWidth * 1.5f);
                    PlayerPrefs.SetInt("voxelRenderDistance", renderDistance);
                    PlayerPrefs.Save();
                }
            }
        }
        public float RenderScale
        {
            get
            {
                return renderScale;
            }
            set
            {
                if (renderScale != value && renderScale >= 0.2f && renderScale <= 1f )
                {
                    renderScale = value;
                    GameRenderSettings.SetRenderScale(renderScale);
                    PlayerPrefs.SetFloat("voxelRenderScale", renderScale);
                    PlayerPrefs.Save();
                }
            }
        }

        public bool DebugGraphy
        {
            get { return debugGraphy; }
            set
            {
                debugGraphy = value;
                PlayerPrefs.SetInt("voxelDebugGraphy", debugGraphy ? 1 : 0);
                PlayerPrefs.Save();
            }
        }    

        public Material ChunksMaterial => chunksMaterial;
        public Material ChunksTranspMaterial => chunksTranspMaterial;
        public VoxelsPresetSO VoxelPresetSO => voxelPresetSO;
        public VoxelStructures Structures => structures;

        public void Init()
        {
            Load();

            structures = new VoxelStructures();
            foreach (var structure in structuresSO)
                Structures.RegistryStructure(structure);

            Voxels.Init(VoxelPresetSO.Voxels);

            if (Seed == -1)
                Seed = Random.Range(0, 1000000);
        }

        public void Load()
        {
            Seed = PlayerPrefs.GetInt("voxelSeed", -1);
            RenderDistance = PlayerPrefs.GetInt("voxelRenderDistance", 12);
            TargetFrameRate = PlayerPrefs.GetInt("voxelTargetFrameRate", 60);
            RenderScale = PlayerPrefs.GetFloat("voxelRenderScale", 1f);
            DebugGraphy = PlayerPrefs.GetInt("voxelDebugGraphy", 0) > 0;
        }

        public void Reset()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
