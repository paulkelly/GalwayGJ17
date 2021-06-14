using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipHitPool : ObjectPool<ShipHit>
{
    public void Spawn(Vector2 position, Vector2 direction)
    {
        ShipHit hit = GetObject();
        hit.Spawn(this, position, direction);
    }
}
