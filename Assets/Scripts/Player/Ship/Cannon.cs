using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    private Ship _parentShip;
    
    private const float InputDeadZone = 0.04f;
    private const float QuaternionRotationTime = 0.1f;
    private const float MaxFireRate = 1/8f;

    public Vector2 ParentSpeed { get; set; }
    public Vector2 Vector { get; set; }
    public float Strength { get; set; }
    
    // Rotation parameters
    private float _inputAngle;
    private Quaternion _targetRotation;
    private Quaternion _rotationVel;
    
    // Shooting parameters
    private float _shotCooldown;

    private void Awake()
    {
        _parentShip = GetComponentInParent<Ship>();
    }

    private void Update()
    {
        _shotCooldown += Time.deltaTime * Strength;
        if (_shotCooldown > MaxFireRate)
        {
            BulletPool.Spawn(_parentShip, _spawnPoint.position, transform.rotation * Vector2.up, ParentSpeed);
            _shotCooldown -= MaxFireRate;
        }
    }
    
    private void FixedUpdate()
    {
        if (Vector.sqrMagnitude > InputDeadZone)
        {
            _inputAngle = Vector2.SignedAngle(Vector, Vector2.up);
        }
        
        _targetRotation = Quaternion.AngleAxis(-_inputAngle, Vector3.forward);

        transform.rotation = QuaternionUtil.SmoothDamp(transform.rotation, _targetRotation, ref _rotationVel,
            QuaternionRotationTime);
    }
}
