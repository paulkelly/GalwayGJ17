using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisHitPool : ObjectPool<DebrisHit>
{
    private static DebrisHitPool _instance;

    private void Awake()
    {
        _instance = this;
    }
    
    public static void Spawn(Vector2 position,Vector2 velocity, float strength)
    {
        if(_instance != null) _instance.SpawnLocal(position, velocity,strength);
    }

    private void SpawnLocal(Vector2 position, Vector2 velocity, float strength)
    {
        DebrisHit hit = GetObject();
        hit.Spawn(this, position, velocity, strength);
    }
}
