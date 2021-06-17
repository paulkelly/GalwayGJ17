using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHit : PooledObject
{
    [SerializeField] ParticleSystem _particleSystem;
    
    private BulletHitPool _pool;

    public void Spawn(BulletHitPool pool, Vector2 position)
    {
        transform.position = position;
        
        _pool = pool;
        _particleSystem.Play();

        StartCoroutine(ReturnWhenFinished());
    }
    
    private WaitForSeconds _waitTime = new WaitForSeconds(1);
    private IEnumerator ReturnWhenFinished()
    {
        yield return _waitTime;
        _pool.ReleaseObject(this);
    }
}
