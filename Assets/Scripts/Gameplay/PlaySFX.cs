using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFX : MonoBehaviour
{
    private const float VolumeRandomness = 0.05f;
    
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private List<AudioClip> _clips;

    public void Play(float volume = 1)
    {
        _audioSource.PlayOneShot(_clips[Random.Range(0, _clips.Count)], Mathf.Clamp01(Random.Range(volume-VolumeRandomness, volume+VolumeRandomness)));
    }
}
