using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Cannon : MonoBehaviourPun
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
        if (!photonView.IsMine) return;
        
        _shotCooldown += Time.deltaTime * Strength;
        if (_shotCooldown > MaxFireRate)
        {
            SpawnBullet();
            _shotCooldown -= MaxFireRate;
        }
    }
    
    public void SpawnBullet()
    {
        photonView.RPC("SpawnBulletRPC", RpcTarget.All, _spawnPoint.position, transform.rotation * Vector2.up, ParentSpeed);
    }

    [PunRPC]
    private void SpawnBulletRPC(Vector3 position, Vector2 forward, Vector2 velocity)
    {
        BulletPool.Spawn(_parentShip, position, forward, velocity);
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
