using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SyncedRigidbody : MonoBehaviourPun
{
    protected Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        RigidybodySyncQueue.Subscribe(this);
    }

    private void OnDisable()
    {
        RigidybodySyncQueue.UnSubscribe(this);
    }
    
    public void Sync()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SyncRPC", RpcTarget.Others, _rigidbody.position, _rigidbody.velocity, _rigidbody.rotation, _rigidbody.angularVelocity, PhotonNetwork.Time);
        }
    }

    [PunRPC]
    private void SyncRPC(Vector2 position, Vector2 velocity, float rotation, float angularVel, double sentTime)
    {
        float lag = Mathf.Abs((float) (PhotonNetwork.Time - sentTime));
            
        _rigidbody.position = position + (velocity*lag);
        _rigidbody.velocity = velocity;
        _rigidbody.rotation = rotation + (angularVel*lag);
        _rigidbody.angularVelocity = angularVel;
        
        Debug.Log("Synced: " + name);
    }
}
