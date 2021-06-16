using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Cannon : MonoBehaviourPun
{
    [SerializeField] private Transform _spawnPoint;
    private Ship _parentShip;
    
    public const float MinEnergy = 1;
    public const float MaxEnergy = 4;
    
    private const float InputDeadZone = 0.04f;
    private const float QuaternionRotationTime = 0.05f;
    private const float MinFireRate = 1f;
    private const float MaxFireRate = 3f;
    private const float MinAngleToFire = 15;

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
        _shotCooldown = 1;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        bool ableToFire = Strength > 0.05f;
        // if (ableToFire)
        // {
        //     ableToFire &= Vector2.Angle(transform.rotation * Vector2.up, Vector) < MinAngleToFire;
        // }
        
        if (ableToFire)
        {
            _shotCooldown += Mathf.Lerp(MinFireRate, MaxFireRate, Strength)*Time.deltaTime;
            if (_shotCooldown >= 0.99)
            {
                SpawnBullet(Mathf.Lerp(MinEnergy, MaxEnergy, Strength));
                _shotCooldown = Mathf.Clamp01(_shotCooldown - 1);
            }
        }
        else
        {
            _shotCooldown = Mathf.Clamp01(_shotCooldown - MinFireRate*Time.deltaTime);
        }
    }
    
    public void SpawnBullet(float energy)
    {
        photonView.RPC("SpawnBulletRPC", RpcTarget.All, _spawnPoint.position, energy, (Vector2)(transform.rotation * Vector2.up), ParentSpeed, PhotonNetwork.Time);
    }

    [PunRPC]
    private void SpawnBulletRPC(Vector3 position, float energy, Vector2 forward, Vector2 velocity, double sentTime)
    {
        float lag = Mathf.Abs((float) (PhotonNetwork.Time - sentTime));
        position += lag * (Vector3) velocity;
        BulletPool.Spawn(_parentShip, energy, position, forward, velocity);
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
