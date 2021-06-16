using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class OrbitSpin : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _earthTransform;
    
    [Header("Default Rotation")]
    [SerializeField] private Vector3 _rotationAxis;
    [SerializeField] private float _roationSpeed;

    [Header("Movement Rotation")]
    [SerializeField] private float _rotationPerXTraveled;
    
    [SerializeField] private Vector3 _rotationYAxis;
    [SerializeField] private float _movementPerYTraveled;
    [SerializeField] private float _rotationPerYTraveled;

    private float _lastPositionX;
    private float _lastPositionY;

    private const float SyncTime= 0.3f;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private Vector3 _positionVel;
    private Quaternion _rotVel;

    private void Start()
    {
        var position = _cameraTransform.position;
        _lastPositionX = position.x;
        _lastPositionY = position.y;
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            var position = _cameraTransform.position;
            float movementX = position.x - _lastPositionX;
            float movementY = position.y - _lastPositionY;
    
            position += Vector3.up * (movementY * _movementPerYTraveled);
            transform.position = position;
            
            transform.Rotate(_rotationAxis, (_roationSpeed*Time.deltaTime)+(movementX*_rotationPerXTraveled), Space.World);
            _earthTransform.Rotate(_rotationYAxis, (movementY*_rotationPerYTraveled), Space.World);
    
            _lastPositionX = position.x;
            _lastPositionY = position.y;

            _targetPosition = position;
            _targetRotation = transform.rotation;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _positionVel, SyncTime);
            transform.rotation = QuaternionUtil.SmoothDamp(transform.rotation, _targetRotation, ref _rotVel, SyncTime);
        }
        
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_targetPosition);
            stream.SendNext(_targetRotation);
        }
        else
        {
            _targetPosition = (Vector3)stream.ReceiveNext();
            _targetRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
