using UnityEngine;

namespace VoxelGame.Input
{
    public class Inputs
    {
        public GameState gameState;
        public bool IsPointerOverUI;
        public bool IsCurrentPointerMouse;
        public bool IsCurrentPointerTouchpad;
        public PlayerInputs PlayerInputs { get; private set; }

        private Inputs()
        {
            PlayerInputs = new PlayerInputs();
        }
    }

    public enum GameState
    {
        Game,
        Menu
    }
}