using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHittable
{
    void Hit(Vector2 impact, Vector2 position, float energy);
}
