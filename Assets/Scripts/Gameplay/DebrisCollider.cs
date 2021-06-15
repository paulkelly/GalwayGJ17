using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisCollider : MonoBehaviour, IHittable
{
    [SerializeField] private Debris _parent;

    public void Hit(Vector2 impact, Vector2 position, float energy)
    {
        _parent.Hit(impact, position, energy);
    }
}
