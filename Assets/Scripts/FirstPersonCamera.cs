using DataTypes;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [SerializeField, Tooltip("The transform of the character that owns this camera")]
    private Transform owner;

    private float _pitch;
    
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        Vector2 movement = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        
        _pitch = Mathf.Clamp(_pitch - movement.y, -80f, 80f);
        
        owner.Rotate(Vector3.up, movement.x);
        transform.localRotation = Quaternion.Euler(_pitch, 0, 0);
    }
}
