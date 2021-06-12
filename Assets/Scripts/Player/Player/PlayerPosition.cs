using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPosition
{
    private const float PositionLerpTime = 0.3f;

    public bool Moving => _lerpTime < PositionLerpTime;
    public float CurrentPosition { get; private set; }

    private float _initalPosition;
    private float _targetPosition;
    private float _lerpTime;

    public PlayerPosition(float initalPosition)
    {
        _initalPosition = initalPosition;
        _targetPosition = initalPosition;
        CurrentPosition = initalPosition;
        _lerpTime = PositionLerpTime;
    }

    public void SetTarget(float target)
    {
        _initalPosition = CurrentPosition;
        _targetPosition = target;
        _lerpTime = 0;
    }

    public void Update()
    {
        _lerpTime += Time.deltaTime;
        CurrentPosition =
            Mathf.LerpAngle(_initalPosition, _targetPosition, Mathf.Clamp01(_lerpTime / PositionLerpTime));
    }
}
