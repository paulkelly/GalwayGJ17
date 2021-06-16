using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitSpin : MonoBehaviour
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

    private void Start()
    {
        var position = _cameraTransform.position;
        _lastPositionX = position.x;
        _lastPositionY = position.y;
    }

    private void Update()
    {
        var position = _cameraTransform.position;
        float movementX = position.x - _lastPositionX;
        float movementY = position.y - _lastPositionY;

        transform.position += Vector3.up * (movementY * _movementPerYTraveled);
        transform.Rotate(_rotationAxis, (_roationSpeed*Time.deltaTime)+(movementX*_rotationPerXTraveled), Space.World);
        _earthTransform.Rotate(_rotationYAxis, (movementY*_rotationPerYTraveled), Space.World);

        _lastPositionX = position.x;
        _lastPositionY = position.y;
    }
}
