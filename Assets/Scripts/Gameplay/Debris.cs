using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debris : MonoBehaviour
{
    [SerializeField] private SpriteMask _spriteMask;

    private const float SpriteMaskActiveTime = 0.5f;

    private float _spriteMaskTime;
    private bool _spriteMaskActive = false;

    private void OnCollisionStay2D(Collision2D other)
    {
        _spriteMaskTime = 0;
        _spriteMaskActive = true;
        _spriteMask.enabled = true;
    }

    private void Update()
    {
        if (_spriteMaskActive)
        {
            _spriteMaskTime += Time.deltaTime;
            if (_spriteMaskTime > SpriteMaskActiveTime)
            {
                _spriteMaskActive = false;
                _spriteMask.enabled = false;
            }
        }
    }
}
