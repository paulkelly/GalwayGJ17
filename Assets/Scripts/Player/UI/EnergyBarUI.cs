using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBarUI : MonoBehaviour
{
    [SerializeField] private Transform _energyTransform;
    [SerializeField] private Transform _thrustTransform;
    [SerializeField] private Transform _cannonTransform;
    [SerializeField] private Transform _shieldTransform;

    private const float UpdateSpeed = 10;

    private static float _targetEnergyAllocation;
    private static float _targetThrustAllocation;
    private static float _targetCannonAllocation;
    private static float _targetShields;

    private float _energy;
    private float _thrust;
    private float _cannon;
    private float _shield;

    public static void UpdateUI(float energyAllocation, float thrustAllocation, float cannonAllocation, float shields)
    {
        _targetEnergyAllocation = energyAllocation;
        _targetThrustAllocation = thrustAllocation;
        _targetCannonAllocation = cannonAllocation;
        _targetShields = shields;
    }

    private void Start()
    {
        _energy = 1;
        _thrust = 0;
        _cannon = 0;
        _shield = 1;
    }

    private void Update()
    {
        _energy = Mathf.MoveTowards(_energy, _targetEnergyAllocation, UpdateSpeed * Time.deltaTime);
        _thrust = Mathf.MoveTowards(_thrust, _targetThrustAllocation, UpdateSpeed * Time.deltaTime);
        _cannon = Mathf.MoveTowards(_cannon, _targetCannonAllocation, UpdateSpeed * Time.deltaTime);
        _shield = Mathf.MoveTowards(_shield, _targetShields, UpdateSpeed * Time.deltaTime);

        _energyTransform.localScale = new Vector3(_energy, 1, 1);
        _thrustTransform.localScale = new Vector3(_thrust, 1, 1);
        _cannonTransform.localScale = new Vector3(_cannon, 1, 1);
        _shieldTransform.localScale = new Vector3(_shield, 1, 1);
    }
}
