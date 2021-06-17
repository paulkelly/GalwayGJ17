using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHitPool : ObjectPool<BulletHit>
{
    private static BulletHitPool _instance;

    private void Awake()
    {
        _instance = this;
    }
    
    public static void Spawn(Vector2 position)
    {
        if(_instance != null) _instance.SpawnLocal(position);
    }

    private void SpawnLocal(Vector2 position)
    {
        BulletHit hit = GetObject();
        hit.Spawn(this, position);
    }
}
