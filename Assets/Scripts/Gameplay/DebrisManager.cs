using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DebrisManager : MonoBehaviour
{
    public static float RandomAngularVelocity => Random.Range(-30, 30);

    [SerializeField] private PlaySFX _debrisHitSfx;
    [SerializeField] private PlaySFX _debrisDestroyedSfx;
    
    [SerializeField] private int _minDebrisTargetCount;
    [SerializeField] private int _maxDebrisTargetCount;
    [SerializeField] private float _debrisSpawnIncreaseTime;
    [SerializeField] private float _defaultSpawnRate;
    
    private static DebrisManager _instance;
    private Dictionary<DebrisType, List<DebrisPool>> _debrisPools = new Dictionary<DebrisType, List<DebrisPool>>();
    private static List<DebrisSpawnZone> _spawnZones = new List<DebrisSpawnZone>();
    
    private float _currentDebrisTargetCount;
    private int _currentDebris;
    private float _nextSpawn = 0;
    
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

    public static void RegisterSpawnZone(DebrisSpawnZone zone)
    {
        _spawnZones.Add(zone);
    }
    
    public static void RemoveSpawnZone(DebrisSpawnZone zone)
    {
        _spawnZones.Remove(zone);
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

        _currentDebrisTargetCount = _minDebrisTargetCount;
    }

    private void Update()
    {
        _currentDebrisTargetCount = Mathf.Clamp(_currentDebrisTargetCount + (Time.deltaTime*_debrisSpawnIncreaseTime), _minDebrisTargetCount,
            _maxDebrisTargetCount);
        if (_currentDebrisTargetCount < 0.1f) return;
        float spawnRate = (_currentDebris / _currentDebrisTargetCount)*_defaultSpawnRate;
        _nextSpawn += Time.deltaTime;
        if (_nextSpawn > spawnRate)
        {
            SpawnDebrisOnRandomSpawnPoint();
            _nextSpawn = 0;
        }
    }

    private void SpawnDebrisOnRandomSpawnPoint()
    {
        _spawnZones[Random.Range(0, _spawnZones.Count)].SpawnDebris();
    }

    private void AddPool(DebrisType type, DebrisPool pool)
    {
        if (!_debrisPools.ContainsKey(type))
        {
            _debrisPools.Add(type, new List<DebrisPool>());
        }
        
        _debrisPools[type].Add(pool);
    }

    private void OnEnable()
    {
        Debris.OnDebrisHit += DebrisOnOnDebrisHit;
        Debris.OnDebrisDestroyed += DebrisOnOnDebrisDestroyed;
        
        Debris.OnDebrisSpawned += DebrisOnOnDebrisSpawned;
        Debris.OnDebrisDespawned += DebrisOnOnDebrisDespawned;
    }
    

    private void OnDisable()
    {
        Debris.OnDebrisHit -= DebrisOnOnDebrisHit;
        Debris.OnDebrisDestroyed -= DebrisOnOnDebrisDestroyed;
        
        Debris.OnDebrisSpawned -= DebrisOnOnDebrisSpawned;
        Debris.OnDebrisDespawned -= DebrisOnOnDebrisDespawned;
    }

    private void DebrisOnOnDebrisHit()
    {
        _debrisHitSfx.Play();
    }
    private void DebrisOnOnDebrisDestroyed(int value)
    {
        _debrisDestroyedSfx.Play();
    }
    
    private void DebrisOnOnDebrisSpawned()
    {
        _currentDebris++;
    }
    private void DebrisOnOnDebrisDespawned()
    {
        _currentDebris--;
        if (_currentDebris < 0) _currentDebris = 0;
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
