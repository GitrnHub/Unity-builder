using UnityEngine;
using UnityEngine.Events;
using Lean.Gui;

namespace VoxelGame.Core
{
    public class PauseMenu : MonoBehaviour
    {
        public UnityEvent OnClosed = new();

        [SerializeField] private GameObject ui;

        public LeanButton btnContinue;
        public LeanButton btnSettings;
        public LeanButton btnExit;

        public void Open(bool isOpen)
        {
            ui.SetActive(isOpen);

            if (!isOpen)
                OnClosed?.Invoke();
        }
    }
}