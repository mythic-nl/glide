using System;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    private const float SourceEngineUnit = 28.07f;
    
    [SerializeField] private float _airAcceleration = 4f;
    [SerializeField] private float _groundAcceleration = 4f;

    [SerializeField] private float _maxAirSpeed = 114f;
    [SerializeField] private float _maxGroundSpeed = 11.4f;

    [SerializeField] private float _surfaceFriction = 1f;
    [SerializeField] private float _friction = 4f;

    [SerializeField] private Vector3 _velocity;
    [SerializeField] private Vector3 _wishDirection;
    
    private void Awake()
    {
        
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Velocity: {_velocity.magnitude}");
    }

    private void Update()
    {
        _wishDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        
        Movement();
        
        // Move towards the rotated camera direction
        _wishDirection = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * _wishDirection;
        _wishDirection.Normalize();
        
        
    }

    private void Movement()
    {
        _velocity = Friction();
        _velocity = Accelerate(_groundAcceleration, _maxGroundSpeed);

        transform.Translate(_velocity * Time.deltaTime, Space.World);
    }

    private Vector3 Accelerate(float acceleration, float maxSpeed)
    {
        float currentSpeed = Vector3.Dot(_velocity, _wishDirection);
        float addSpeed = acceleration - currentSpeed;

        if (currentSpeed + addSpeed > maxSpeed) {
            addSpeed = maxSpeed - currentSpeed;
        }

        return _velocity + _wishDirection * addSpeed;
    }
    
    private Vector3 Friction()
    {
        float speed = _velocity.magnitude;
        
        if (speed < 0.1f) {
            return Vector3.zero;
        }

        float drop = speed * _friction * Time.deltaTime;
        return _velocity *= Mathf.Max(speed - drop, 0.0f) / speed;
    }
}