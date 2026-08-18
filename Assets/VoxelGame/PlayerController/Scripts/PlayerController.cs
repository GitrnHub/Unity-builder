using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VoxelGame.Voxel;
using VoxelGame.Input;
using VoxelGame.Core;
using Zenject;
using Cinemachine;

namespace VoxelGame.Player
{
    [RequireComponent(typeof(CharacterBoxController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Move")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float rotationSmoothTime = 0.15f;
        [SerializeField] private float rotationSensetivity = 1f;
        [SerializeField] private float speedChangeRate = 10f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -15f;
        [SerializeField] private float jumpTimeout = 0.5f;
        [SerializeField] private float fallTimeout = 0.15f;
        
        [Header("Camera")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private float cameraFov = 70f;
        [SerializeField] private float cameraFovSprint = 80f;
        [SerializeField] private float topClamp = 85f;
        [SerializeField] private float bottomClamp = -85f;
        [SerializeField] private float cameraAngle = 0f;
        [SerializeField] private bool lockCameraPosition = false;
        [SerializeField] private float cameraWalkOffsetMultiplier = 1f;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        [Header("Footsteps")]
        [SerializeField] private RandomSound rndSoundFootsteps;

        [Header("Actions")]

        // camera target walk offset
        private float cameraAmplitudeGain;

        // cinemachine
        private float cinemachineTargetYaw;
        private float cinemachineTargetPitch;

        // player
        private float speed;
        private float targetRotation = 0.0f;
        private float rotationVelocity;
        private float verticalVelocity;
        private float terminalVelocity = 53.0f;
        private bool runMode;

        // timeout deltatime
        private float jumpTimeoutDelta;
        private float fallTimeoutDelta;

        private const float threshold = 0.01f;

        private CharacterBoxController controller;
        private Transform mainCamera;
        private PlayerAction action;

        [Inject]
        private readonly Inputs input;

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
            }

            cinemachineTargetYaw = cameraTarget.rotation.eulerAngles.y;
            controller = GetComponent<CharacterBoxController>();
            action = GetComponent<PlayerAction>();
            action.SetCamera(mainCamera);

            jumpTimeoutDelta = jumpTimeout;
            fallTimeoutDelta = fallTimeout;

            input.PlayerInputs.Fire.OnStarted.AddListener(() =>
            {
                if (!input.IsPointerOverUI)
                {
                    action.Dig();
                }
            });

            input.PlayerInputs.FireAlt.OnStarted.AddListener(() =>
            {
                if (!input.IsPointerOverUI)
                {
                    action.Build();
                }
            });
        }

        private void Update()
        {
            JumpAndGravity();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void JumpAndGravity()
        {
            if (controller.IsGrounded)
            {
                fallTimeoutDelta = fallTimeout;

                if (verticalVelocity < 0.0f)
                {
                    verticalVelocity = -2f;
                }

                // Jump
                if (input.PlayerInputs.Jump.IsPressed && jumpTimeoutDelta <= 0.0f)
                {
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }

                // jump timeout
                if (jumpTimeoutDelta >= 0.0f)
                {
                    jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                jumpTimeoutDelta = jumpTimeout;

                if (fallTimeoutDelta >= 0.0f)
                {
                    fallTimeoutDelta -= Time.deltaTime;
                }
            }

            if (verticalVelocity < terminalVelocity)
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }

        private void Move()
        {
            Vector2 moveInput = input.PlayerInputs.Move.Value;
            float inputMagnitude = moveInput.magnitude;

            if (inputMagnitude > 0.5f && input.PlayerInputs.Run.IsPressed && controller.IsGrounded)
                runMode = true;

            if (inputMagnitude <0.25f && runMode)
                runMode = false;

            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, runMode ? cameraFovSprint : cameraFov, Time.deltaTime * 2f);
            float targetSpeed = 0.0f;

            if (moveInput != Vector2.zero)
            {
                targetSpeed = runMode ? sprintSpeed : moveSpeed;
            }

            float currentHorizontalSpeed = new Vector3(controller.Velocity.x, 0.0f, controller.Velocity.z).magnitude;

            float speedOffset = 0.1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * speedChangeRate);

                speed = Mathf.Round(speed * 1000f) / 1000f;
            }
            else
            {
                speed = targetSpeed;
            }

            Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

            if (moveInput != Vector2.zero)
            {
                targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                    rotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

            controller.Move(targetDirection.normalized * (speed * Time.deltaTime) +
                             new Vector3(0.0f, verticalVelocity * Time.deltaTime, 0.0f));

            // cancel jump if block above
            if (controller.HitHead)
            {
                verticalVelocity = Mathf.Min(verticalVelocity, 0f);
            }

            cameraAmplitudeGain = Mathf.MoveTowards(cameraAmplitudeGain, controller.IsGrounded ? speed * cameraWalkOffsetMultiplier : 0f, Time.deltaTime * 4);
            virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = cameraAmplitudeGain;

            if (speed > 0.5f && controller.IsGrounded)
            {
                rndSoundFootsteps.PlayRandomSound(2.75f / speed);
            }
        }

        private void CameraRotation()
        {
            Vector2 lookInput = input.PlayerInputs.Look.Value;

            if (lookInput.sqrMagnitude >= threshold && !lockCameraPosition)
            {
                float deltaTimeMultiplier = input.IsCurrentPointerMouse || input.IsCurrentPointerTouchpad ? 1.0f : Time.deltaTime * 180f;

                cinemachineTargetYaw += lookInput.x * deltaTimeMultiplier * rotationSensetivity;
                cinemachineTargetPitch += lookInput.y * deltaTimeMultiplier * rotationSensetivity;
            }

            cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
            cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

            cameraTarget.rotation = Quaternion.Euler(
                cinemachineTargetPitch + cameraAngle,
                cinemachineTargetYaw,
                0.0f);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}