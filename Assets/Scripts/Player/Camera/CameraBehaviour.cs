using System;
using Input;
using Player.Structs;
using UnityEngine;
using Utils;

namespace Player.Camera
{
    public class CameraBehaviour : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInput input;
        
        [Header("Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 3f;
        [SerializeField] private float sensitivity = 1.0f;
        [SerializeField] private float minPitch = -30.0f;
        [SerializeField] private float maxPitch = 60.0f;
        [SerializeField] private float followPlanarResponse = 16f;
        [SerializeField] private float followVerticalResponse = 8f;

        private InputRequest _inputRequest;

        private float _yaw;
        private float _pitch;
        private float _distance;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            transform.position = target.position - transform.forward * distance;
        }
        
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            _inputRequest.LookDirection = input.Look;
            _yaw   += _inputRequest.LookDirection.x * sensitivity * deltaTime;
            _pitch -= _inputRequest.LookDirection.y * sensitivity * deltaTime;
            
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }
        
        private void LateUpdate()
        {
            if (target == false) {
                Debug.LogError("CameraBehaviour: No target assigned for the camera.");
                return;
            }

            float deltaTime = Time.deltaTime;
            
            Vector3 desiredPosition = target.position - transform.forward * distance;
            Vector3 planarPosition = Vector3.Lerp(
                a: transform.position,
                b: desiredPosition,
                t: Common.GetInterpolationTime(followPlanarResponse, deltaTime)
            );
            float verticalPosition = Mathf.Lerp(
                a: transform.position.y,
                b: desiredPosition.y,
                t: Common.GetInterpolationTime(followVerticalResponse, deltaTime)
            );

            transform.eulerAngles = new Vector3(_pitch, _yaw);
            transform.position = new Vector3(planarPosition.x, verticalPosition, planarPosition.z);
        }

        private void OnDrawGizmosSelected()
        {
            if (target == false) return;
            
            transform.position = target.position - transform.forward * distance;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(target.position, transform.position);
            Gizmos.DrawSphere(transform.position, 0.1f);
        }
    }
}