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

    private AudioSource _audioSource;

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
        _audioSource.PlayOneShot(brokenSound);
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
