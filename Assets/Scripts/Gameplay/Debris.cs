using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class Debris : MonoBehaviourPun, IHittable, IPooledObject
{
    public delegate void DebrisDestoryed(int value);
    public static event DebrisDestoryed OnDebrisDestroyed;

    public delegate void DebrisHit();

    public static event DebrisHit OnDebrisHit;
    
    public delegate void DebrisSpawned();

    public static event DebrisSpawned OnDebrisSpawned;
    public delegate void DebrisDespawned();

    public static event DebrisDespawned OnDebrisDespawned;
    
    [SerializeField] private int PointValue;
    [SerializeField] private float _maxHealth;
    [SerializeField] private List<SpawnOnDestroy> _splitInto;

    private Rigidbody2D _rigidbody;
    private DebrisPool _pool;
    
    private bool _alive;
    private float _currentHealth;

    private bool _hasShipReference;
    private Ship _ship;
    
    private void Awake()
    {
        if(_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
        _alive = true;
        _currentHealth = _maxHealth;
    }

    private void Update()
    {
        if (_hasShipReference)
        {
            if (Vector2.Distance(_ship.Position, _rigidbody.position) > 60)
            {
                Despawn();
            }
        }
    }

    public void Spawn(DebrisPool pool, Vector2 position, Vector2 velocity, float rotation, float angularVelocity)
    {
        if(_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
        
        _pool = pool;
        _alive = true;
        _currentHealth = _maxHealth;

        transform.position = position;
        transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
        
        _rigidbody.position = position;
        _rigidbody.velocity = velocity;
        _rigidbody.rotation = rotation;
        _rigidbody.angularVelocity = angularVelocity;

        if (photonView.IsMine)
        {
            _ship = Ship.Instance;
            _hasShipReference = true;
        }
        
        OnDebrisSpawned?.Invoke();
    }

    public void Hit(Vector2 impact, Vector2 position, float energy)
    {
        if (!_alive) return;
        
        _currentHealth -= energy;
        _rigidbody.AddForceAtPosition(impact * energy, position);
        DebrisHitPool.Spawn(position, impact, energy);
        if (_currentHealth <= 0)
        {
            Kill();
        }
        else
        {
            OnDebrisHit?.Invoke();
        }
    }

    private void Kill()
    {
        OnDebrisDestroyed?.Invoke(PointValue);
        if (photonView.IsMine)
        {
            float splitVelocity = Random.Range(1.5f, 3.5f);
            foreach (var split in _splitInto)
            {
                Vector2 awayVelocity = ((Vector2) split.Position.position-_rigidbody.position) * splitVelocity;
                float randomAngularVelocity = DebrisManager.RandomAngularVelocity;
                DebrisManager.Spawn(split.Type, split.Position.position, _rigidbody.velocity + awayVelocity,
                    _rigidbody.rotation, _rigidbody.angularVelocity + randomAngularVelocity);
            }
        }

        Despawn();
    }

    private void Despawn()
    {
        if (!photonView.IsMine) return;
        
        if (_alive)
        {
            OnDebrisDespawned?.Invoke();
            _alive = false;
        }

        if (_pool == null)
        {
            Destroy(gameObject);
        }
        else
        {
            _pool.ReleaseObject(this);   
        } 
    }

    #region IObjectPool
    public void Get()
    {
        photonView.RPC("EnableRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void EnableRPC()
    {
        gameObject.SetActive(true);
    }

    
    public void Release()
    {
        photonView.RPC("ReleaseRPC", RpcTarget.AllBuffered);
    }
    
    [PunRPC]
    private void ReleaseRPC()
    {
        gameObject.SetActive(false);
    }
    #endregion
}

[Serializable]
public class SpawnOnDestroy
{
    public DebrisManager.DebrisType Type;
    public Transform Position;
}
