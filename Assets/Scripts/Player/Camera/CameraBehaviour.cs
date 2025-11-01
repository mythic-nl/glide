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

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            transform.position = target.position - transform.forward * distance.Value;
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

            float planarSmoothTime = Common.GetInterpolationTime(followPlanarResponse.Value, deltaTime);
            float verticalSmoothTime = Common.GetInterpolationTime(followVerticalResponse.Value, deltaTime);

            Vector3 planarDesiredTarget = new Vector3(desiredPosition.x, transform.position.y, desiredPosition.z);
            Vector3 planarPosition = Vector3.Lerp(
                a: transform.position,
                b: planarDesiredTarget,
                t: planarSmoothTime
            );            
            
            float verticalPosition = Mathf.Lerp(
                a: transform.position.y,
                b: desiredPosition.y,
                t: verticalSmoothTime
            );
            
            transform.rotation = desiredRotation;
            transform.position = new Vector3(planarPosition.x, verticalPosition, planarPosition.z);
        }

        private void OnDrawGizmosSelected()
        {
            if (target == false) return;

#if UNITY_EDITOR
            if (Application.isPlaying == false) {
                // Do not modify transform here. Compute a preview position and draw gizmos from it.
                Quaternion previewRotation = Quaternion.Euler(_pitch, _yaw, 0f);
                Vector3 previewPos = target.position - (previewRotation * Vector3.forward) * distance.Value;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(target.position, previewPos);
                Gizmos.DrawSphere(previewPos, 0.1f);
            }
#endif
        }
    }
}