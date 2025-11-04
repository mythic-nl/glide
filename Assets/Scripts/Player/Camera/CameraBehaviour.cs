using System;
using Input;
using Player.Structs;
using ScriptableObjects;
using UnityEngine;
using Utils;

namespace Player.Camera
{
    public class CameraBehaviour : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInput input;
        [SerializeField] private Transform target;
        
        [Header("Settings")]
        [SerializeField] private FloatVariable distance;
        [SerializeField] private FloatVariable sensitivity;
        [SerializeField] private FloatVariable minPitch;
        [SerializeField] private FloatVariable maxPitch;
        [SerializeField] private FloatVariable followPlanarResponse;
        [SerializeField] private FloatVariable followVerticalResponse;

        private InputRequest _inputRequest;

        private float _yaw;
        private float _pitch;
        private float _distance;

        // Track last known target Y to detect vertical movement of the target itself
        private float _lastTargetY;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (target == null) {
                Debug.LogError("CameraBehaviour: No target assigned for the camera.");
                return;
            }

            // Position camera at the desired distance along current forward
            transform.position = target.position - transform.forward * distance.Value;

            // Initialize last target Y for vertical movement detection
            _lastTargetY = target.position.y;
        }
        
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            _inputRequest.LookDirection = input.Look;
            _yaw   += _inputRequest.LookDirection.x * sensitivity.Value * deltaTime;
            _pitch -= _inputRequest.LookDirection.y * sensitivity.Value * deltaTime;
            
            _pitch = Mathf.Clamp(_pitch, minPitch.Value, maxPitch.Value);
        }
        
        private void LateUpdate()
        {
            if (target == false) {
                Debug.LogError("CameraBehaviour: No target assigned for the camera.");
                return;
            }

            float deltaTime = Time.deltaTime;

            Quaternion desiredRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 desiredPosition = target.position - (desiredRotation * Vector3.forward) * distance.Value;
            
            float verticalSmoothing = Mathf.Lerp(
                a: transform.position.y,
                b: desiredPosition.y,
                t: Common.GetInterpolationTime(followVerticalResponse.Value, deltaTime)
            );
            
            transform.rotation = desiredRotation;
            transform.position = new Vector3(desiredPosition.x, verticalSmoothing, desiredPosition.z);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, distance.Value);

            if (target == false) {
                return;
            }
            
            if (Application.isPlaying == false) {
                Quaternion previewRotation = Quaternion.Euler(_pitch, _yaw, 0f);
                Vector3 previewPos = target.position - (previewRotation * Vector3.forward) * distance.Value;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(target.position, previewPos);
                Gizmos.DrawSphere(previewPos, 0.1f);
            }
        }
    }
}