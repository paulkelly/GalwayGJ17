using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class Debris : MonoBehaviourPun, IHittable
{
    [SerializeField] private float _maxHealth;
    [SerializeField] private List<GameObject> _splitObjects;

    private bool _alive;
    private float _currentHealth;
    
    protected Rigidbody2D _rigidbody;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        _alive = true;
        _currentHealth = _maxHealth;
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
        foreach (var splitObj in _splitObjects)
        {
            splitObj.transform.parent = null;
            splitObj.SetActive(true);
            
            Rigidbody2D splitBody = splitObj.GetComponent<Rigidbody2D>();
            if (splitBody != null)
            {
                splitBody.velocity = _rigidbody.velocity;
                splitBody.angularVelocity = _rigidbody.angularVelocity;
                splitBody.AddForce((transform.position - splitObj.transform.position) * Random.Range(1000, 1400));
                splitBody.AddTorque(Random.Range(-100, 100));
            }
        }
        Destroy(gameObject);
    }
    
    //
    //
    // protected Rigidbody2D _rigidbody;
    //
    // private void Awake()
    // {
    //     _rigidbody = GetComponent<Rigidbody2D>();
    // }
    //
    // private void OnEnable()
    // {
    //     RigidybodySyncQueue.Subscribe(this);
    // }
    //
    // private void OnDisable()
    // {
    //     RigidybodySyncQueue.UnSubscribe(this);
    // }
    //
    // public void Sync()
    // {
    //     if (photonView.IsMine)
    //     {
    //         photonView.RPC("SyncRPC", RpcTarget.Others, _rigidbody.position, _rigidbody.velocity, _rigidbody.rotation, _rigidbody.angularVelocity, PhotonNetwork.Time);
    //     }
    // }
    //
    // [PunRPC]
    // private void SyncRPC(Vector2 position, Vector2 velocity, float rotation, float angularVel, double sentTime)
    // {
    //     float lag = Mathf.Abs((float) (PhotonNetwork.Time - sentTime));
    //         
    //     _rigidbody.position = position + (velocity*lag);
    //     _rigidbody.velocity = velocity;
    //     _rigidbody.rotation = rotation + (angularVel*lag);
    //     _rigidbody.angularVelocity = angularVel;
    //     
    //     Debug.Log("Synced: " + name);
    // }
}
