using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PooledObject
{
    [SerializeField] private float _despawnDistance;
    [SerializeField] private float _speed;
    private Ship _ship;
    private Vector2 _forwardVector;
    private Vector2 _inheritedVelocity;
    private Action _despawnCallback;

    public void Spawn(Ship ship, Vector3 position, Vector2 forward, Vector2 inheritedVelocity, Action despawnCallback)
    {
        _ship = ship;
        transform.position = position;
        _forwardVector = forward;
        _inheritedVelocity = inheritedVelocity;
        _despawnCallback = despawnCallback;

        float angle = Vector2.SignedAngle(forward, Vector2.up);
        transform.rotation =  Quaternion.AngleAxis(-angle, Vector3.forward);
    }
    
    public void Update()
    {
        Vector2 moveVector = ((_speed * _forwardVector)+ _inheritedVelocity) * Time.deltaTime;
        Vector3 position = transform.position;
        transform.position = new Vector3(position.x + moveVector.x, position.y + moveVector.y,
            position.z);

        if (Vector3.Distance(_ship.transform.position, position) > _despawnDistance)
        {
            _despawnCallback?.Invoke();
        }
    }
}
