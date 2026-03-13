using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class SoundMaster : MonoBehaviour
{
    public static SoundMaster instance;

    [SerializeField] private AudioSource audioSourcePrefab;
    public bool muted = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private AudioSource setupAudio(AudioClip audio, Transform t)
    {
        AudioSource audioSource = Instantiate(audioSourcePrefab, t);

        audioSource.volume = !muted ? 1 : 0;
        audioSource.clip = audio;

        return audioSource;
    }

    public void playAudio(AudioClip audio, Transform t)
    {
        var audioSource = setupAudio(audio, t);

        audioSource.pitch = UnityEngine.Random.Range(.2f, .9f);

        audioSource.Play();

        Destroy(audioSource, audioSource.clip.length);
    }

    public void playGameOverAudio(AudioClip audio, Transform t)
    {
        var audioSource = setupAudio(audio, t);

        audioSource.Play();

        Destroy(audioSource, audioSource.clip.length);
    }

    public void playOnPerfectAudio(AudioClip audio, Transform t, int score)
    {
        var audioSource = setupAudio(audio, t);

        audioSource.pitch = .7f + (score * 0.10f);

        audioSource.Play();

        Destroy(audioSource, audioSource.clip.length);
    }
}
