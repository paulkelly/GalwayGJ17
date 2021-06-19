using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = System.Numerics.Vector3;

[RequireComponent(typeof(Collider2D))]
public class DebrisSpawnZone : MonoBehaviourPun
{
    [SerializeField] private Vector2 _towardsCenter;
    private Collider2D _collider;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        //StartCoroutine(SpawnCo());
    }

    private void OnEnable()
    {
        DebrisManager.RegisterSpawnZone(this);
    }

    private void OnDisable()
    {
        DebrisManager.RemoveSpawnZone(this);
    }

    public void SpawnDebris()
    {
        if (photonView.IsMine)
        {
            Vector2 position = new Vector2(Random.Range(_collider.bounds.min.x, _collider.bounds.max.x),
                Random.Range(_collider.bounds.min.y, _collider.bounds.max.y));

            DebrisManager.DebrisType type = GetRandomTye(); 
            Vector2 velocityDirection = _towardsCenter + new Vector2(Random.Range(-0.8f, 0.8f), Random.Range(-0.8f, 0.8f));

            velocityDirection = velocityDirection.normalized * GetVelocityMultiplier(type);
            
            DebrisManager.Spawn(type, position, velocityDirection, Random.Range(0f, 360f),
                DebrisManager.RandomAngularVelocity);
        }
    }

    private DebrisManager.DebrisType GetRandomTye()
    {
        float d20 = Random.Range(0, 20);

        if (d20 < 4)
        {
            return DebrisManager.DebrisType.Large;
        }
        if (d20 < 9)
        {
            return DebrisManager.DebrisType.Medium;
        }
        if (d20 < 10)
        {
            return DebrisManager.DebrisType.ChunkA;
        }
        if (d20 < 11)
        {
            return DebrisManager.DebrisType.ChunkB;
        }
        
        return DebrisManager.DebrisType.Small;
    }

    private float GetVelocityMultiplier(DebrisManager.DebrisType type)
    {
        return type switch
        {
            DebrisManager.DebrisType.Large => Mathf.Lerp(0, 4, Random.Range(0, 1f)),
            DebrisManager.DebrisType.Medium => Mathf.Lerp(2, 8, Random.Range(0, 1f)),
            DebrisManager.DebrisType.Small => Mathf.Lerp(4, 16, Random.Range(0, 1f)),
            _ => Mathf.Lerp(0, 4, Random.Range(0, 1f))
        };
    }
    
    // private IEnumerator SpawnCo()
    // {
    //     //TODO: Wait until player has spawned
    //     while (true)
    //     {
    //         yield return new WaitForSeconds(Random.Range(1, 4));
    //         SpawnDebris();
    //     }
    // }
}
