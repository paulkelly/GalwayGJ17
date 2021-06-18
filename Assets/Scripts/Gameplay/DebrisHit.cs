using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DebrisHit : PooledObject
{
    private const float ParticlesPerCollisionStrength = 0.5f;
    private const float MinParticles = 4;
    private const float MaxParticles = 8;
    
    // private const float ImpactSpawnVelocity = 1;
    // private const float RandomSpawnVelocity = 2;
    
    [SerializeField] private List<ParticleSystem> _particleSystems;
    
    private DebrisHitPool _pool;
    
    public void Spawn(DebrisHitPool pool, Vector2 position, Vector2 velocity, float strength)
    {
        _pool = pool;
        
        transform.position = position;
        int toSpawn = Mathf.FloorToInt(Mathf.Clamp(strength * ParticlesPerCollisionStrength, MinParticles, MaxParticles));

        int[] emitPerSystem = new int[_particleSystems.Count];
        for (int i = 0; i < toSpawn; i++)
        {
            emitPerSystem[Random.Range(0, emitPerSystem.Length)]++;
        }

        //velocity = Vector2.ClampMagnitude((velocity), ImpactSpawnVelocity);
        for (int i = 0; i < emitPerSystem.Length; i++)
        {
            //Vector2 particleVel = velocity + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * RandomSpawnVelocity;
            //_particleSystems[i].Emit(new ParticleSystem.EmitParams{velocity = particleVel}, emitPerSystem[i]);
            _particleSystems[i].Emit(emitPerSystem[i]);
        }

        StartCoroutine(ReturnWhenFinished());
    }

    private WaitForSeconds _waitTime = new WaitForSeconds(5);
    private IEnumerator ReturnWhenFinished()
    {
        yield return _waitTime;
        _pool.ReleaseObject(this);
    }
}
