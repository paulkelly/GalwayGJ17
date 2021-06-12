using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : ObjectPool<Bullet>
{
    private static BulletPool _instance;

    public static void Spawn(Vector3 position, Vector2 forward)
    {
        if (_instance != null)
        {
            Bullet spawnedBullet = _instance.GetObject();
            spawnedBullet.Spawn(position, forward);
        }
    }

    public override void OnAwake()
    {
        _instance = this;
    }
}
