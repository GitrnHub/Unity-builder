using UnityEngine;
using Zenject;

namespace VoxelGame.Core
{
    public class GameMenu : MonoBehaviour
    {
        [Inject] public CanvasFader fader;
        [Inject] public SettingsMenu settings;
        [Inject] public PauseMenu pause;
        [Inject] public InventoryMenu inventory;

        private void Start()
        {
            settings.Open(false);
            pause.Open(false);
            inventory.Open(false);

            settings.SetResetButton(false);
        }
    }
}
