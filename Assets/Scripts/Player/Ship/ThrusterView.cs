using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrusterView : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    private ParticleSystem.EmissionModule _emissionModule;
    private float _defaultRate;
    
    public float Thrust { get; set; }

    private void Start()
    {
        _emissionModule = _particleSystem.emission;
        _defaultRate = _emissionModule.rateOverTime.constant;
    }

    private void Update()
    {
        _emissionModule.rateOverTime = _defaultRate * Thrust;
    }
}
