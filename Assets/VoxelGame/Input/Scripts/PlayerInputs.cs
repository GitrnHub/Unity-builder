using UnityEngine;
using UnityEngine.Events;

namespace VoxelGame.Input
{
    public class PlayerInputs
    {
        public InputLocker inputLocker = new();
        public InputValue<float> Fire;
        public InputValue<float> FireAlt;
        public InputValue<float> Jump;
        public InputValue<float> Run;
        public InputValue<float> Escape;
        public InputValue<float> Inventory;
        public InputValue<Vector2> Move;
        public InputValue<Vector2> Look;

        public PlayerInputs()
        {
            Fire = new InputValue<float>(inputLocker);
            FireAlt = new InputValue<float>(inputLocker);
            Jump = new InputValue<float>(inputLocker);
            Run = new InputValue<float>(inputLocker);

            Move = new InputValue<Vector2>(inputLocker);
            Look = new InputValue<Vector2>(inputLocker);

            Escape = new InputValue<float>(null);
            Inventory = new InputValue<float>(null);
        }
    }

    public class InputLocker
    {
        public UnityEvent<bool> OnChanged = new();
        
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (value != isEnabled)
                {
                    OnChanged?.Invoke(value);
                    isEnabled = value;
                }
            }
        }
        private bool isEnabled;
    }
}