using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : PooledObject
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private CircleCollider2D _collider;
    [SerializeField] private float _despawnDistance;
    [SerializeField] private float _speed;
    [SerializeField] private float _impactForce;
    
    [SerializeField] private Color _lowEnergyColor;
    [SerializeField] private Color _highEnergyColor;
    
    public float Energy { get; private set; }

    private Rigidbody2D _rigidbody;
    private ParticleSystem.EmissionModule _emissionModule;
    private ParticleSystem.MainModule _mainModule;

    private Ship _ship;
    private Vector2 _forwardVector;
    private Vector2 _inheritedVelocity;
    private Vector2 _velocity;
    private Action _despawnCallback;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Spawn(Ship ship, float energy, Vector3 position, Vector2 forward, Vector2 inheritedVelocity, Action despawnCallback)
    {
        _ship = ship;
        Energy = energy;
        float normalEnergy = Mathf.InverseLerp(Cannon.MinEnergy, Cannon.MaxEnergy, Energy);
        _collider.radius = Mathf.Lerp(0.5f, 0.8f, normalEnergy);
        _mainModule = _particleSystem.main;
        _mainModule.startLifetimeMultiplier = Mathf.Lerp(0.1f, 0.3f, normalEnergy);
        _mainModule.startSizeMultiplier = Mathf.Lerp(0.3f, 1.2f, normalEnergy);
        _mainModule.startColor = Color.Lerp(_lowEnergyColor, _highEnergyColor, normalEnergy);

        transform.position = position;
        _rigidbody.position = position;
        _forwardVector = forward;
        _inheritedVelocity = inheritedVelocity;
        _despawnCallback = despawnCallback;

        float angle = Vector2.SignedAngle(forward, Vector2.up);
        transform.rotation =  Quaternion.AngleAxis(-angle, Vector3.forward);
        
        //_velocity =(_speed * _forwardVector)+ _inheritedVelocity;
        _velocity =(_speed * _forwardVector);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IHittable hittable = other.GetComponent<IHittable>();
        if (hittable != null)
        {
            hittable.Hit(_velocity * _impactForce, other.ClosestPoint(transform.position), Energy);
            OnHit();
        }
    }

    public void OnHit()
    {
        BulletHitPool.Spawn(_rigidbody.position);
        _despawnCallback?.Invoke();
    }

    private void Update()
    {
        if (Vector3.Distance(_ship.transform.position, transform.position) > _despawnDistance)
        {
            _despawnCallback?.Invoke();
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.velocity = _velocity;
    }
}
