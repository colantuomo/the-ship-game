using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField]
    private AudioClip _changeSize, _hitShip, _impulse, _death;
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayChangeSize()
    {
        _audioSource.PlayOneShot(_changeSize);
    }

    public void PlayHitShip()
    {
        _audioSource.PlayOneShot(_hitShip);
    }

    public void PlayImpulse()
    {
        _audioSource.PlayOneShot(_impulse);
    }

    public void PlayDeath()
    {
        _audioSource.PlayOneShot(_death);
    }
}
