using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DebrisManager : MonoBehaviour
{
    public static float RandomAngularVelocity => Random.Range(-30, 30);
    
    private static DebrisManager _instance;
    private Dictionary<DebrisType, List<DebrisPool>> _debrisPools = new Dictionary<DebrisType, List<DebrisPool>>();
    
    public static DebrisType Any
    {
        get
        {
            var v = Enum.GetValues (typeof (DebrisType));
            return (DebrisType) v.GetValue(Random.Range(0, v.Length));
        }
    }
    public static void Spawn(DebrisType type, Vector2 position, Vector2 velocity, float rotation, float angularVelocity)
    {
        if (_instance != null)
        {
            _instance.SpawnLocal(type, position, velocity, rotation, angularVelocity);
        }
    }

    private void SpawnLocal(DebrisType type, Vector2 position, Vector2 velocity, float rotation, float angularVelocity)
    {
        if (_debrisPools.ContainsKey(type))
        {
            DebrisPool randomPool = _debrisPools[type][Random.Range(0, _debrisPools[type].Count)];
            if (randomPool.IsReady)
            {
                Debris debris = randomPool.GetObject(position, Quaternion.AngleAxis(rotation, Vector3.forward));
                if(debris != null) debris.Spawn(randomPool, position, velocity, rotation, angularVelocity);
            }
        }
    }

    private void SpawnRPC(DebrisType type, Vector2 position, Vector2 velocity, float rotation, float angularVelocity)
    {
        
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        List<DebrisPool> allPools = new List<DebrisPool>();
        allPools.AddRange(GetComponentsInChildren<DebrisPool>());

        foreach (var pool in allPools)
        {
            AddPool(pool.Type, pool);
        }
    }

    private void AddPool(DebrisType type, DebrisPool pool)
    {
        if (!_debrisPools.ContainsKey(type))
        {
            _debrisPools.Add(type, new List<DebrisPool>());
        }
        
        _debrisPools[type].Add(pool);
    }

    public enum DebrisType
    {
        Large,
        Medium,
        Small,
        ChunkA,
        ChunkB
    }
}
