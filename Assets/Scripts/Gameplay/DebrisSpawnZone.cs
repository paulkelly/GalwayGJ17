using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Collider2D))]
public class DebrisSpawnZone : MonoBehaviour
{
    private Collider2D _collider;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        StartCoroutine(SpawnCo());
    }

    private void SpawnDebris()
    {
        Vector2 position = new Vector2(Random.Range(_collider.bounds.min.x, _collider.bounds.max.x),Random.Range(_collider.bounds.min.y, _collider.bounds.max.y));
        DebrisManager.Spawn(DebrisManager.Any, position, new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)), 0, DebrisManager.RandomAngularVelocity);
    }

    private IEnumerator SpawnCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1, 4));
            SpawnDebris();
        }
    }
}
