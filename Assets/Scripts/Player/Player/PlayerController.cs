using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Rewired;
using UnityEngine;
using Action = RewiredConsts.Action;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    private const float InputDeadZone = 0.3f;
    
    [SerializeField]
    private PlayerIndicator _playerIndicator;
    [SerializeField]
    private PlayerInputFeedback _inputFeedback;
    [SerializeField] 
    private SpriteRenderer _myPlayerSpriteRenderer;
    
    
    private Player _player;
    
    private Vector2 ThrustDirection { get; set; }
    private Vector2 ShootDirection { get; set; }
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
            // InputDirection = _player.GetAxis2D(Action.AimX, Action.AimY);
            // Thrust = _player.GetAxis(Action.Thrust);
            // Cannons = _player.GetAxis(Action.Shoot);

            ThrustDirection = _player.GetAxis2D(Action.ThrustX, Action.ThrustY);
            ShootDirection = _player.GetAxis2D(Action.ShootX, Action.ShootY);
            
            Thrust = ThrustDirection.magnitude;
            Cannons = ShootDirection.magnitude;
        }

        _playerIndicator.ThrustDirection = ThrustDirection;
        _playerIndicator.ShootDirection = ShootDirection;
        _playerIndicator.Thrust = Thrust;
        _playerIndicator.Cannons = Cannons;
        
        _inputFeedback.Thrusting = Thrust > 0.3f;
        _inputFeedback.Shooting = Cannons > 0.3f;
        _inputFeedback.Moving = (Cannons > InputDeadZone || Thrust > InputDeadZone);
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ThrustDirection);
            stream.SendNext(ShootDirection);
        }
        else
        {
            ThrustDirection = (Vector2)stream.ReceiveNext();
            ShootDirection = (Vector2)stream.ReceiveNext();
            Thrust = ThrustDirection.magnitude;
            Cannons = ShootDirection.magnitude;
        }
    }

}
