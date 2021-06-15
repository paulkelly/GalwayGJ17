using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    private ParticleSystem.EmissionModule _emissionModule;
    private ParticleSystem.MainModule _mainModule;
    private float _defaultRate;
    
    public float Thrust { get; set; }

    private void Start()
    {
        _emissionModule = _particleSystem.emission;
        _mainModule = _particleSystem.main;
        _defaultRate = _emissionModule.rateOverTime.constant;
    }

    private void Update()
    {
        _mainModule.startLifetimeMultiplier = Mathf.Lerp(0.2f, 0.6f, Thrust);
        _mainModule.startSizeMultiplier = Mathf.Lerp(0.3f, 1.5f, Thrust*Thrust);
        if (Thrust > 0.05f)
        {
            _emissionModule.rateOverTime = _defaultRate;
        }
        else
        {
            _emissionModule.rateOverTime = 0;
        }
    }
}
