using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using Zenject;

namespace VoxelGame.Core
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject uiMenu;
        [SerializeField] private GameObject uiLoading;

        [Inject] private readonly CanvasFader canvasFader;
        [Inject] private readonly SettingsMenu settingsMenu;

        void Start()
        {
            canvasFader.Fade(false);
            settingsMenu.OnClosed.AddListener(() => uiMenu.SetActive(true));

            settingsMenu.Open(false);
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.isPressed)
            {
                ExitGame();
            }
        }

        public void OpenSettings()
        {
            uiMenu.SetActive(false);
            settingsMenu.Open(true);
        }

        public void LoadGame()
        {
            uiLoading.SetActive(true);

            canvasFader.Fade(true, () =>
            {
                SceneManager.LoadSceneAsync("game");
            });
        }

        public void ExitGame()
        {
            uiLoading.SetActive(true);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}
