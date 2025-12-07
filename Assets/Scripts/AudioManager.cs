using NUnit.Framework.Internal.Commands;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip bgmClip;
    public AudioClip walkClip;
    public AudioClip placeClip;
    public AudioClip deleteClip;

    void Awake()
    {
        // Biar AudioManager cuma ada 1 di seluruh game
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        PlayBGM(); // Mainkan musik saat game mulai
    }

    // === BGM ===
    public void PlayBGM()
    {
        if (bgmSource && bgmClip)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 0.05f;
            bgmSource.Play();
        }
    }

    // === SFX ===
    public void PlayWalkSFX() => PlaySFX(walkClip);
    public void PlayPlaceSFX() => PlaySFX(placeClip);
    public void PlayDeleteSFX() => PlaySFX(deleteClip);

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource && clip)
        {
            sfxSource.volume = 1f; // pastikan efek suara tetap penuh
            sfxSource.PlayOneShot(clip);
        }
    }
}
