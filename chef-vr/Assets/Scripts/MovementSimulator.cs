using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;

namespace VRChef
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class MovementSimulator : MonoBehaviour
    {

        [SerializeField] private MouseLook mouseLook;
        [SerializeField] private float moveSpeed = 2.0f;
        [SerializeField] private float moveSpeedCrouchedModifier = 0.6f;
        [SerializeField] private float moveSpeedRunningModifier = 2.0f;
        [SerializeField] private float stepInterval = 5.0f;
        [SerializeField] private float moveSpeedSmoothingAlpha = 0.2f;
        [SerializeField] private float mouseMoveSensitivity = 0.01f;
        [SerializeField] private float crouchedHeight = -0.5f;
        [SerializeField] private float heightSmoothingAlpha = 0.2f;
        [SerializeField] private AudioClip[] footstepSounds;
        [SerializeField] private KeyCode keyMoveForward = KeyCode.W;
        [SerializeField] private KeyCode keyMoveBackward = KeyCode.S;
        [SerializeField] private KeyCode keyMoveLeft = KeyCode.A;
        [SerializeField] private KeyCode keyMoveRight = KeyCode.D;
        [SerializeField] private KeyCode keyCPad = KeyCode.LeftAlt;
        [SerializeField] private KeyCode keyCrouch = KeyCode.LeftControl;
        [SerializeField] private KeyCode keyRun = KeyCode.LeftShift;
        [SerializeField] private GameObject hotbox;
        [SerializeField] private GameObject hotboxCircle;
        [SerializeField] private AnimationCurve hotboxDeltaCurve = AnimationCurve.Linear(0, 1, 1, 0);

        private Camera camera;
        private CharacterController characterController;
        private AudioSource audioSource;

        private float currentMoveSpeed = 0.0f;
        private Vector2 hotboxPosition = Vector2.zero;
        private Vector3 defaultHotboxPosition;

        // Use this for initialization
        private void Start()
        {
            camera = Camera.main;
            characterController = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
            mouseLook.Init(transform, camera.transform);
            defaultHotboxPosition = hotboxCircle.transform.localPosition;
        }

        // Update is called once per frame
        private void Update()
        {
            bool isMovingWithMouse = Input.GetKey(keyCPad);

            if (!isMovingWithMouse)
            {
                //mouse rotates camera
                mouseLook.LookRotation(transform, camera.transform);

                //reset hotbox
                hotboxPosition = Vector2.zero;
            } else
            {
                //mouse moves character
                Vector2 mouseDelta = Vector2.zero;
                mouseDelta.x = CrossPlatformInputManager.GetAxis("Mouse X") * mouseMoveSensitivity;
                mouseDelta.y = CrossPlatformInputManager.GetAxis("Mouse Y") * mouseMoveSensitivity;

                Vector2 newHotboxPosition = hotboxPosition + mouseDelta;
                float updateMagnitude = hotboxDeltaCurve.Evaluate(newHotboxPosition.magnitude);
                hotboxPosition += mouseDelta * updateMagnitude;
            }

            //update hotbox UI
            Image hotboxImg = hotbox.GetComponent<Image>();
            Color hotboxColor = hotboxImg.color;
            hotboxColor.a = isMovingWithMouse ? 1f : 0.3f;
            hotboxImg.color = hotboxColor;
            Image hotboxCenterImg = hotboxCircle.GetComponent<Image>();
            hotboxCenterImg.color = hotboxPosition.magnitude > 0 ? Color.green : Color.gray;
            hotboxCircle.transform.localPosition = defaultHotboxPosition + new Vector3(hotboxPosition.x * 50f, hotboxPosition.y * 50f, 0);
        }

        private void FixedUpdate()
        {
            Vector2 directionalInput = Vector2.zero;

            if (!Input.GetKey(keyCPad))
            {
                //keys move
                directionalInput.x -= Input.GetKey(keyMoveLeft) ? 1 : 0;
                directionalInput.x += Input.GetKey(keyMoveRight) ? 1 : 0;
                directionalInput.y += Input.GetKey(keyMoveForward) ? 1 : 0;
                directionalInput.y -= Input.GetKey(keyMoveBackward) ? 1 : 0;

                //mouse looks, see Update()
            } else {
                //mouse moves character
                directionalInput.x = hotboxPosition.x;
                directionalInput.y = hotboxPosition.y;
            }

            //process move speed
            bool moving = directionalInput.magnitude > 0;
            float moveSpeed = moving ? this.moveSpeed : 0;
            if (Input.GetKey(keyCrouch))
            {
                moveSpeed *= moveSpeedCrouchedModifier;
            }
            else if (Input.GetKey(keyRun))
            {
                moveSpeed *= moveSpeedRunningModifier;
            }
            float actualMoveSpeed = moveSpeedSmoothingAlpha * moveSpeed + (1 - moveSpeedSmoothingAlpha) * currentMoveSpeed;
            currentMoveSpeed = moveSpeed;

            //move player
            directionalInput *= actualMoveSpeed;
            Vector3 desiredMove = transform.forward * directionalInput.y + transform.right * directionalInput.x;
            transform.position += desiredMove;

            //TODO player physics

            //process player height
            Vector3 currentPosition = transform.position;
            float targetHeight = Input.GetKey(keyCrouch) ? crouchedHeight : 0f;
            float newHeight = targetHeight * heightSmoothingAlpha + (1f - heightSmoothingAlpha) * currentPosition.y;
            transform.position = new Vector3(currentPosition.x, newHeight, currentPosition.z);

            //ensure cursor is locked
            mouseLook.UpdateCursorLock();
        }

        private void ProgressStepCycle(float speed)
        {
            //if (characterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            //{
            //    m_StepCycle += (characterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
            //                 Time.fixedDeltaTime;
            //}

            //if (!(m_StepCycle > m_NextStep))
            //{
            //    return;
            //}

            //m_NextStep = m_StepCycle + stepInterval;

            //PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!characterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = UnityEngine.Random.Range(1, footstepSounds.Length);
            audioSource.clip = footstepSounds[n];
            audioSource.PlayOneShot(audioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            footstepSounds[n] = footstepSounds[0];
            footstepSounds[0] = audioSource.clip;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            
        }
    }
}