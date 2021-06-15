using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : ObjectPool<Bullet>
{
    private static BulletPool _instance;

    public static void Spawn(Ship ship, float energy, Vector3 position, Vector2 forward, Vector2 inheritedVelocity)
    {
        if (_instance != null)
        {
            Bullet spawnedBullet = _instance.GetObject();
            spawnedBullet.Spawn(ship, energy, position, forward, inheritedVelocity, () =>
            {
                _instance.ReleaseObject(spawnedBullet);
            });
        }
    }

    public override void OnAwake()
    {
        _instance = this;
    }
}
