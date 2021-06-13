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
    [SerializeField]
    private PlayerInputFeedback _inputFeedback;
    [SerializeField] 
    private SpriteRenderer _myPlayerSpriteRenderer;
    
    private Player _player;
    
    private Vector2 InputDirection { get; set; }
    private float Thrust { get; set; }
    private float Cannons { get; set; }
    private void Start()
    {
        if (photonView.IsMine)
        {
            _player = ReInput.players.GetPlayer(0);
            _myPlayerSpriteRenderer.enabled = true;
        }
        else
        {
            _myPlayerSpriteRenderer.enabled = false;
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

        _inputFeedback.Moving = InputDirection.sqrMagnitude > PlayerIndicator.InputDeadZone;
        _inputFeedback.Thrusting = Thrust > 0.3f;
        _inputFeedback.Shooting = Cannons > 0.3f;
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
