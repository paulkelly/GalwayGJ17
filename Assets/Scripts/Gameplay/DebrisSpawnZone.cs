using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Collider2D))]
public class DebrisSpawnZone : MonoBehaviourPun
{
    private Collider2D _collider;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        StartCoroutine(SpawnCo());
    }

    private void SpawnDebris()
    {
        if (photonView.IsMine)
        {
            Vector2 position = new Vector2(Random.Range(_collider.bounds.min.x, _collider.bounds.max.x),
                Random.Range(_collider.bounds.min.y, _collider.bounds.max.y));
            DebrisManager.Spawn(DebrisManager.Any, position, new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)), 0,
                DebrisManager.RandomAngularVelocity);
        }
    }

    private IEnumerator SpawnCo()
    {
        //TODO: Wait until player has spawned
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1, 4));
            SpawnDebris();
        }
    }
}
