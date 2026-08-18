using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

using VoxelGame.Voxel;

namespace VoxelGame.Core
{
    public class InventoryMenu : MonoBehaviour
    {
        public UnityEvent OnOpened = new();
        public UnityEvent OnClosed = new();

        public UnityEvent<int> OnItemSelect = new();

        [SerializeField] private GameObject uiPanel;
        [SerializeField] private RectTransform buttonsPanel;
        [SerializeField] private GameObject buttonPrefab;

        [Inject]
        private readonly VoxelSettingsSO voxelSettings;

        private List<UIInventoryButton> buttons;

        private void Start()
        {
            buttons = new List<UIInventoryButton>();

            if (voxelSettings == null)
                return;

            if (voxelSettings.VoxelPresetSO == null)
                return;

            for (int i = 1; i < voxelSettings.VoxelPresetSO.Voxels.Length; i++)
            {
                if (voxelSettings.VoxelPresetSO.Voxels[i] == null)
                    continue;

                if (voxelSettings.VoxelPresetSO.Voxels[i].name == "")
                    continue;

                GameObject go = Instantiate(buttonPrefab, buttonsPanel);
                go.name = $"{voxelSettings.VoxelPresetSO.Voxels[i].name}";
                UIInventoryButton btn = go.GetComponent<UIInventoryButton>();

                btn.Set(Voxels.voxels[i].name, Voxels.voxels[i].sprite);

                int id = i;

                btn.Button.onClick.AddListener(() =>
                {
                    SelectItem(id);
                });

                buttons.Add(btn);
            }

            if (buttons.Count > 0)
            {
                buttons[0].gameObject.AddComponent<UIFirstSelect>();
            }
        }

        public void Open(bool isOpen)
        {
            uiPanel.SetActive(isOpen);

            if (isOpen)
                OnOpened?.Invoke();
            else
                OnClosed?.Invoke();
        }

        public bool SwithOpen()
        {
            bool isOpen = !uiPanel.activeSelf;

            Open(isOpen);

            return isOpen;
        }

        private void SelectItem(int id)
        {
            OnItemSelect?.Invoke(id);
        }
    }
}