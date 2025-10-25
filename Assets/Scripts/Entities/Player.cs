using Input;
using Unity.VisualScripting;
using UnityEngine;

namespace Entities
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter playerCharacter;
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private CameraSpring cameraSpring;
        [SerializeField] private CameraLean cameraLean;
        [SerializeField] private CameraFOV cameraFOV;
        
        private InputSystemActions _inputActions;

        private void OnEnable()
        {
            _inputActions = new InputSystemActions();
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Dispose();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            
            playerCharacter.Initialize();
            playerCamera.Initialize(playerCharacter.GetCameraTarget());
            cameraSpring.Initialize();
            cameraLean.Initialize();
            cameraFOV.Initialize();
        }

        private void Update()
        {
            var input = _inputActions.Player;
            var deltaTime = Time.deltaTime; 
            
            CameraInput cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
            playerCamera.UpdateRotation(cameraInput);

            var characterInput = new CharacterInput {
                Rotation = playerCamera.transform.rotation,
                MovementDirection = input.Move.ReadValue<Vector2>(),
                Jump = input.Jump.WasPressedThisFrame(),
                JumpSustain = input.Jump.IsPressed(),
                Crouch = input.Crouch.WasPressedThisFrame() ? CrouchInput.Toggle : CrouchInput.None
            };
            
            playerCharacter.UpdateInput(characterInput);
            playerCharacter.UpdateBody(deltaTime);
            
            #if UNITY_EDITOR
            if (UnityEngine.Input.GetKeyDown(KeyCode.T)) {
                var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                if (Physics.Raycast(ray, out var hitInfo)) {
                    Teleport(hitInfo.point);
                }
            }
            #endif
        }

        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            var cameraTarget = playerCharacter.GetCameraTarget();
            var state = playerCharacter.GetState();
            
            playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());
            cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
            cameraLean.UpdateLean(deltaTime, state.Acceleration, cameraTarget.up);
            cameraFOV.UpdateFOV(deltaTime, state.Acceleration.magnitude, state.Stance is Stance.Slide);
        }

        private void Teleport(Vector3 position)
        {
            playerCharacter.SetPosition(position);
        }
    }
}
