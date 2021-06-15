using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class Debris : MonoBehaviour, IHittable
{
    [SerializeField] private float _maxHealth;
    [SerializeField] private List<GameObject> _splitObjects;

    private Rigidbody2D _rigidbody;
    
    private bool _alive;
    private float _currentHealth;

    private void Awake()
    {
        Spawn();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Spawn()
    {
        _alive = true;
        _currentHealth = _maxHealth;
    }

    public void Hit(Vector2 impact, Vector2 position, float energy)
    {
        if (!_alive) return;
        _currentHealth -= energy;
        _rigidbody.AddForceAtPosition(impact * energy, position);
        if (_currentHealth < 0)
        {
            Kill();
        }
    }

    private void Kill()
    {
        foreach (var splitObj in _splitObjects)
        {
            splitObj.transform.parent = null;
            splitObj.SetActive(true);
            
            Rigidbody2D splitBody = splitObj.GetComponent<Rigidbody2D>();
            if (splitBody != null)
            {
                splitBody.velocity = _rigidbody.velocity;
                splitBody.angularVelocity = _rigidbody.angularVelocity;
                splitBody.AddForce((transform.position - splitObj.transform.position) * Random.Range(1000, 1400));
                splitBody.AddTorque(Random.Range(-100, 100));
            }
        }
        Destroy(gameObject);
    }
}
