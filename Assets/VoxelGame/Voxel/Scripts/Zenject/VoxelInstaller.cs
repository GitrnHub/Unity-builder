using Zenject;
using UnityEngine;

namespace VoxelGame.Voxel
{
    public class VoxelInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindVoxel();
        }

        private void BindVoxel()
        {
            Container.Bind<IGenerator>().To<DefaultGenerator>().AsSingle();
            Container.Bind<IChunkControl>().To<ChunkControl>().AsSingle();
            Container.Bind<IChunkView>().To<ChunkView>().FromNew().AsTransient().Lazy();
            Container.BindFactory<Vector2Int, Chunk, Chunk.Factory>();
            Container.Bind<ChunkSystem>().FromComponentInHierarchy().AsSingle();
        }
    }
}
