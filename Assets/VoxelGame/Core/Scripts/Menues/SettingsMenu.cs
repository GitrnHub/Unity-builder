using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Lean.Gui;
using Zenject;

using VoxelGame.Voxel;
using Tayx.Graphy;

namespace VoxelGame.Core
{
    public class SettingsMenu : MonoBehaviour
    {
        public UnityEvent OnOpened = new();
        public UnityEvent OnClosed = new();

        [SerializeField] private GameObject ui;

        [Inject] private readonly VoxelSettingsSO voxelSettings;
        [Inject] private readonly ISaveLoadChunk saveLoad;

        [SerializeField] private Slider sliderRenderDistance;
        [SerializeField] private Slider sliderRenderScale;
        [SerializeField] private GameObject togglesFps;
        [SerializeField] private LeanToggle toggleFps30;
        [SerializeField] private LeanToggle toggleFps60;
        [SerializeField] private LeanToggle toggleFps120;
        [SerializeField] private LeanToggle toggleDebugGraphy;
        [SerializeField] private LeanButton reset;

        public void Open(bool isOpen)
        {
            ui.SetActive(isOpen);

            if (isOpen)
                OnOpened?.Invoke();
            else
                OnClosed?.Invoke();

            UpdateButtons();
        }

        public void SetRenderDistance(float distance)
        {
            voxelSettings.RenderDistance = Mathf.Clamp(Mathf.FloorToInt(distance), 6, 18);
        }

        public void SetTargetFrameRate(int targetFrameRate)
        {
            voxelSettings.TargetFrameRate = targetFrameRate;
        }

        public void SetRenderScale(float renderScale)
        {
            voxelSettings.RenderScale = Mathf.Clamp(renderScale, 0.3f, 1f);
        }

        public void SetDebugGraphy(bool isEnabled)
        {
            voxelSettings.DebugGraphy = isEnabled;

            if (GraphyManager.Instance == null)
                return;

            if (isEnabled)
                GraphyManager.Instance.Enable();
            else
                GraphyManager.Instance.Disable();
        }

        public void SetResetButton(bool isEnabled)
        {
            reset.gameObject.SetActive(isEnabled);
        }

        private void UpdateButtons()
        {
            sliderRenderDistance.value = voxelSettings.RenderDistance;
            sliderRenderScale.value = voxelSettings.RenderScale;

            toggleFps30.Set(voxelSettings.TargetFrameRate == 30);
            toggleFps60.Set(voxelSettings.TargetFrameRate == 60);
            toggleFps120.Set(voxelSettings.TargetFrameRate == 120);

            toggleDebugGraphy.Set(voxelSettings.DebugGraphy);

            togglesFps.SetActive(Application.platform == RuntimePlatform.Android);
        }

        public void Reset()
        {
            saveLoad.DeleteAll();
            voxelSettings.Reset();
            voxelSettings.Load();
            UpdateButtons();
        }
    }
}