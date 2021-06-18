using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RCSThruster : MonoBehaviour
{
    private const float PlayTime = 0.05f;
    private const float EmitRate = 1/30f;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private ParticleSystem _particleSystem;

    private Vector3 _ourVector;
    public float Rotation { get; set; }

    private float _remainingPlayTime = 0;
    private float _emitTime = 0;

    private void Start()
    {
        _ourVector = transform.localPosition;
    }

    public void Update()
    {
        bool hasVelocity = Mathf.Abs(Rotation) > 10f;

        if (hasVelocity)
        {
            _remainingPlayTime = PlayTime;
        }

        _remainingPlayTime -= Time.deltaTime;
        bool play = _remainingPlayTime > 0;

        if (_particleSystem.isPlaying != play)
        {
            if (play)
            {
                _particleSystem.Play();
                _audioSource.Play();
            }
            else
            {
                _particleSystem.Stop();
                _audioSource.Stop();
            }
        }
        
        if (hasVelocity)
        {
            _emitTime += Time.deltaTime;
            //transform.localRotation = Quaternion.AngleAxis(90 * Mathf.Sign(Rotation), _ourVector);
            if (_emitTime > EmitRate)
            {
                Vector3 particleVelocity = Quaternion.AngleAxis(90 * Mathf.Sign(Rotation), _ourVector) * Vector3.forward;
                //particleVelocity = Quaternion.Inverse(transform.rotation) * particleVelocity;
                _particleSystem.Emit(new ParticleSystem.EmitParams {velocity = particleVelocity * 3}, 1);

                _emitTime -= EmitRate;
            }
        }
    }
}
