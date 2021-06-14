using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipHit : PooledObject
{
    [SerializeField] private ParticleSystem _particleSystem;
    private ShipHitPool _pool;
    public void Spawn(ShipHitPool pool, Vector2 position, Vector2 direction)
    {
        _pool = pool;
        transform.position = position;
        float angle = Vector2.SignedAngle(direction, Vector2.up);
        transform.rotation =  Quaternion.AngleAxis(-angle, Vector3.forward);
        _particleSystem.Play();
        
        StopAllCoroutines();
        StartCoroutine(ReleaseAfterTime(Shield.ShieldShowTime));
    }

    private IEnumerator ReleaseAfterTime(float showTime)
    {
        float time = 0;
        while (time < showTime)
        {
            time += Time.deltaTime;
            yield return null;
        }
        
        _pool.ReleaseObject(this);
    }
}
