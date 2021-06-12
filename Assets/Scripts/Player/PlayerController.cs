using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Rewired;
using UnityEngine;
using Action = RewiredConsts.Action;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [SerializeField]
    private PlayerIndicator _playerIndicator;
    
    private Player _player;
    
    private Vector2 InputDirection { get; set; }
    private float Thrust { get; set; }
    private float Cannons { get; set; }
    private void Start()
    {
        if (photonView.IsMine)
        {
            _player = ReInput.players.GetPlayer(0);
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            InputDirection = _player.GetAxis2D(Action.AimX, Action.AimY);
            Thrust = _player.GetAxis(Action.Thrust);
            Cannons = _player.GetAxis(Action.Shoot);
        }

        _playerIndicator.InputDirection = InputDirection;
        _playerIndicator.Thrust = Thrust;
        _playerIndicator.Cannons = Cannons;
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(InputDirection);
            stream.SendNext(Thrust);
            stream.SendNext(Cannons);
        }
        else
        {
            InputDirection = (Vector2)stream.ReceiveNext();
            Thrust = (float)stream.ReceiveNext();
            Cannons = (float)stream.ReceiveNext();
        }
    }

}
