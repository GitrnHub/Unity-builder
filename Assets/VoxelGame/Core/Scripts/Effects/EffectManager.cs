using UnityEngine;
using Zenject;

using VoxelGame.Voxel;

namespace VoxelGame.Core
{
    public class EffectManager : MonoBehaviour
    {
        [Inject]
        private readonly ChunkSystem chunkSystem;

        public static EffectManager Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                instance = FindObjectOfType<EffectManager>();

                if (instance != null)
                    return instance;

                return instance;
            }
        }

        private static EffectManager instance;

        [SerializeField] private GameObject effectBlockDig;

        private void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            chunkSystem.OnVoxelDestroyed.AddListener((pos, voxel) =>
            {
                CreateEffectBlockDig(pos + Vector3.one * 0.5f, Voxels.voxels[voxel].color);
            });
        }

        public void CreateEffectBlockDig(Vector3 position, Color color)
        {
            GameObject go = Instantiate(effectBlockDig, position, Quaternion.identity);

            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor = color;

            go.transform.SetParent(transform);
        }
    }
}
