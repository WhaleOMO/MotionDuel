using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioClip brokenSound;
    public AudioClip bgmMusic;
    public AudioClip[] skillSoundClip;
    public float initialPitch = 0.5f;

    public AudioSource _audioSource;
    public AudioSource _brokenSource;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
        instance = this;
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = bgmMusic;
        PlayBackgroundMusic();

    }

    public void PlayBrokenSound()
    {
        if(initialPitch > 1.5f)
        {
            initialPitch = 0.5f;
        }
        _brokenSource.pitch = initialPitch;
        _brokenSource.PlayOneShot(brokenSound);
        initialPitch += 0.2f;
    }

    public void PlaySkillSound(int skillIndex)
    {
        _audioSource.PlayOneShot(skillSoundClip[skillIndex]);
    }


    void PlayBackgroundMusic()
    {
        if (bgmMusic != null && _audioSource != null)
        {
            _audioSource.loop = true;

            // ≤•∑≈±≥æ∞“Ù¿÷
            _audioSource.Play();
        }
        else
        {
            Debug.LogError("Background music clip or AudioSource component is missing!");
        }
    }
}
