using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace VoxelGame.Input.BaseInput
{
    public class BaseInput : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private ControlStyle controlStyle;
        [SerializeField] private bool lockCursor;
        [SerializeField] private bool unlockCursorOnAndroid;

        [SerializeField] List<GameObject> uiKeyboardMouse;
        [SerializeField] List<GameObject> uiTouch;
        [SerializeField] List<GameObject> uiGamepad;

        private bool cursorLocked = false;

        [Inject]
        private readonly Inputs inputs;

        internal enum ControlStyle
        {
            None,
            KeyboardMouse,
            Touch,
            Gamepad,
        }

        [ContextMenu("Change to UI map")]
        private void ChangeToUIMap()
        {
            playerInput.SwitchCurrentActionMap("UI");
        }

        [ContextMenu("Change Player map")]
        private void ChangeToPlayerMap()
        {
            playerInput.SwitchCurrentActionMap("Player");
        }

        private void Start()
        {
            if (unlockCursorOnAndroid && Application.platform == RuntimePlatform.Android)
            {
                lockCursor = false;
            }
            
            playerInput.controlsChangedEvent.AddListener((call) => OnControlsChanched());

            OnControlsChanched();

            PlayerInputBinding.Bind(playerInput, inputs.PlayerInputs);
        }

        private void Update()
        {
            CheckPointerOverUI();
            LockCursor();
        }

        private void CheckPointerOverUI()
        {
            inputs.IsPointerOverUI = controlStyle == ControlStyle.KeyboardMouse && EventSystem.current.IsPointerOverGameObject();
        }

        private void LockCursor()
        {
            if (lockCursor && inputs.gameState == GameState.Game)
            {
                if (!cursorLocked)
                    SetCursorLockState(true);
            }
            else
            {
                if (cursorLocked)
                    SetCursorLockState(false);
            }
        }

        public void OnControlsChanched()
        {
            if (playerInput.GetDevice<Touchscreen>() != null)
                controlStyle = ControlStyle.Touch;
            else if (playerInput.GetDevice<Mouse>() != null)
                controlStyle = ControlStyle.KeyboardMouse;
            else if (playerInput.GetDevice<Gamepad>() != null)
                controlStyle = ControlStyle.Gamepad;
            else
                Debug.LogError("Control scheme not founded:" + playerInput.currentControlScheme);

            inputs.IsCurrentPointerMouse = controlStyle == ControlStyle.KeyboardMouse;

            SetActive(uiKeyboardMouse, controlStyle == ControlStyle.KeyboardMouse);
            SetActive(uiGamepad, controlStyle == ControlStyle.Gamepad);
            SetActive(uiTouch, controlStyle == ControlStyle.Touch);
        }

        private void SetActive(List<GameObject> gameObjects, bool isActive)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject != null)
                {
                    gameObject.SetActive(isActive);
                }
            }
        }

        private void SetCursorLockState(bool newState)
        {
            cursorLocked = newState;
            Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !cursorLocked;
        }
    }
}