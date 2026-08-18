using Zenject;
using VoxelGame.Input;

namespace VoxelGame.Core
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindInput();
            BindMenues();
        }

        private void BindInput()
        {
            Container.Bind<Inputs>().AsSingle();
        }

        private void BindMenues()
        {
            Container.Bind<CanvasFader>().FromComponentInHierarchy().AsSingle();
            Container.Bind<SettingsMenu>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PauseMenu>().FromComponentInHierarchy().AsSingle();
            Container.Bind<InventoryMenu>().FromComponentInHierarchy().AsSingle();
            Container.Bind<GameMenu>().FromComponentInHierarchy().AsSingle();
        }
    }
}