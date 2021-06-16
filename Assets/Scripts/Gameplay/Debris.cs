using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class Debris : MonoBehaviourPun, IHittable, IPooledObject
{
    [SerializeField] private float _maxHealth;
    [SerializeField] private List<SpawnOnDestroy> _splitInto;

    private Rigidbody2D _rigidbody;
    private DebrisPool _pool;
    
    private bool _alive;
    private float _currentHealth;
    
    
    private void Awake()
    {
        if(_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
        _alive = true;
        _currentHealth = _maxHealth;
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
    }

    public void Hit(Vector2 impact, Vector2 position, float energy)
    {
        if (!_alive) return;
        _currentHealth -= energy;
        _rigidbody.AddForceAtPosition(impact * energy, position);
        if (_currentHealth < 0)
        {
            Kill();
        }
    }

    private void Kill()
    {
        if (photonView.IsMine)
        {
            float splitVelocity = Random.Range(0.75f, 2f);
            foreach (var split in _splitInto)
            {
                Vector2 awayVelocity = (_rigidbody.position - (Vector2) split.Position.position) *
                                       Random.Range(splitVelocity - 0.5f, splitVelocity + 0.5f);
                float randomAngularVelocity = DebrisManager.RandomAngularVelocity;
                DebrisManager.Spawn(split.Type, split.Position.position, _rigidbody.velocity + awayVelocity,
                    _rigidbody.rotation, _rigidbody.angularVelocity + randomAngularVelocity);
            }
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
