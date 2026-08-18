using Zenject;

namespace VoxelGame.Core
{
    public class MenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindMenues();
        }

        private void BindMenues()
        {
            Container.Bind<CanvasFader>().FromComponentInHierarchy().AsSingle();
            Container.Bind<SettingsMenu>().FromComponentInHierarchy().AsSingle();
        }
    }
}