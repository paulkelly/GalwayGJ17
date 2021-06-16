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
    private Vector3 _positionVel;
    
    private Quaternion _targetRotation;
    private Quaternion _rotVel;
    
    private Quaternion _earthRotation;
    private Quaternion _earthRotVel;

    private void Start()
    {
        var position = _cameraTransform.position;
        _lastPositionX = position.x;
        _lastPositionY = position.y;
        
        _targetPosition = transform.localPosition;
        _targetRotation = transform.localRotation;
        _earthRotation = _earthTransform.localRotation;
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            _targetPosition = transform.localPosition;
        }
        
        var position = _cameraTransform.position;
        float movementX = position.x - _lastPositionX;
        float movementY = position.y - _lastPositionY;
    
        _targetPosition += Vector3.up * (movementY * _movementPerYTraveled);

        _lastPositionX = position.x;
        _lastPositionY = position.y;

        _targetRotation *= Quaternion.AngleAxis((_roationSpeed * Time.deltaTime) + (movementX * _rotationPerXTraveled), _rotationAxis);
        _earthRotation *= Quaternion.AngleAxis((movementY*_rotationPerYTraveled), _rotationYAxis);
        //transform.Rotate(_rotationAxis, (_roationSpeed*Time.deltaTime)+(movementX*_rotationPerXTraveled), Space.World);
        //_earthTransform.Rotate(_rotationYAxis, (movementY*_rotationPerYTraveled), Space.World);
        
        if (photonView.IsMine)
        {
            transform.localPosition = _targetPosition;
            transform.localRotation = _targetRotation;
            _earthTransform.localRotation = _earthRotation;
        }
        else
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, _targetPosition, ref _positionVel, SyncTime);
            transform.localRotation = QuaternionUtil.SmoothDamp(transform.localRotation, _targetRotation, ref _rotVel, SyncTime);
            _earthTransform.localRotation = QuaternionUtil.SmoothDamp(_earthTransform.localRotation, _earthRotation, ref _earthRotVel, SyncTime);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_targetPosition);
            stream.SendNext(_targetRotation);
            stream.SendNext(_earthRotation);
        }
        else
        {
            _targetPosition = (Vector3)stream.ReceiveNext();
            _targetRotation = (Quaternion)stream.ReceiveNext();
            _earthRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
