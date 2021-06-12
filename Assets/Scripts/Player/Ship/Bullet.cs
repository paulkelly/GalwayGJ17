using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PooledObject
{
    [SerializeField] private float _speed;
    private Vector2 forwardVector;

    public void Spawn(Vector3 position, Vector2 forward)
    {
        transform.position = position;
        forwardVector = forward;

        float angle = Vector2.SignedAngle(forward, Vector2.up);
        transform.rotation =  Quaternion.AngleAxis(-angle, Vector3.forward);
    }
    
    public void Update()
    {
        Vector2 moveVector = _speed * Time.deltaTime * forwardVector;
        Vector3 position = transform.position;
        transform.position = new Vector3(position.x + moveVector.x, position.y + moveVector.y,
            position.z);
    }
}
