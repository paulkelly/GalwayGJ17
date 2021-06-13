using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteColor : MonoBehaviour
{
    [SerializeField] private Color _inactiveColor;
    [SerializeField] private Color _activeColor;
    [SerializeField] private Color _thrustingColor;
    [SerializeField] private Color _shootingColor;
    [SerializeField] private Color _inactiveThrustingColor;
    [SerializeField] private Color _inactiveShootingColor;
    public enum SpriteState
    {
        Inactive,
        Active,
        Thrusting,
        Shooting,
        InactiveThrusting,
        InactiveShooting
    }

    private const float ColorLerpTime = 0.3f;

    private SpriteState _state;
    private Color _fromColor;
    private Color _targetColor;
    private bool _lerping;
    private float _lerpTime;
    public SpriteState State
    {
        get { return _state; }
        set
        {
            if (_state != value)
            {
                _state = value;
                _fromColor = _spriteRenderer.color;
                _targetColor = _colorTable[_state];
                _lerpTime = 0;
                _lerping = true;
            }
        }
    }

    private Dictionary<SpriteState, Color> _colorTable;
    
    
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _colorTable = new Dictionary<SpriteState, Color>
        {
            { SpriteState.Inactive, _inactiveColor},
            { SpriteState.Active,  _activeColor},
            { SpriteState.Thrusting,  _thrustingColor},
            { SpriteState.Shooting,  _shootingColor},
            { SpriteState.InactiveThrusting,  _inactiveThrustingColor},
            { SpriteState.InactiveShooting,  _inactiveShootingColor}
        };
        State = SpriteState.Inactive;
        _targetColor = _colorTable[_state];
        _spriteRenderer.color = _targetColor;
        _lerpTime = ColorLerpTime;
        _lerping = false;
    }

    private void Update()
    {
        if (!_lerping) return;
        if (_lerpTime < ColorLerpTime)
        {
            _lerpTime += Time.deltaTime;
            _spriteRenderer.color = Color.Lerp(_fromColor, _targetColor, Mathf.Clamp01(_lerpTime/ColorLerpTime));
        }
        else
        {
            _spriteRenderer.color = _targetColor;
            _lerping = false;
        }
    }
}
