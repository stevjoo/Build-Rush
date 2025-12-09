using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource; // boleh saja tidak dipakai lagi untuk SFX, gapapa

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
            // Hormati toggle BGM kalau kamu pakai ToggleAudio (key "BGM_ON")
            bool isBgmOn = PlayerPrefs.GetInt("BGM_ON", 1) == 1;

            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 0.015f;
            bgmSource.mute = !isBgmOn;

            if (isBgmOn && !bgmSource.isPlaying)
                bgmSource.Play();
        }
    }

    // --- bisa kamu panggil dari UI kalau mau refresh setelah toggle ---
    public void RefreshBGMMuteFromPrefs()
    {
        if (!bgmSource) return;
        bool isBgmOn = PlayerPrefs.GetInt("BGM_ON", 1) == 1;
        bgmSource.mute = !isBgmOn;
        if (isBgmOn && !bgmSource.isPlaying && bgmClip != null)
            bgmSource.Play();
    }

    // === SFX wrapper (tetap ada) ===
    public void PlayWalkSFX()        => PlaySFX3D(walkClip, GetListenerPosition());
    public void PlayPlaceSFX()       => PlaySFX3D(placeClip, GetListenerPosition());
    public void PlayDeleteSFX()      => PlaySFX3D(deleteClip, GetListenerPosition());

    // Versi 3D yang dipanggil MovementController / BuilderController
    public void PlayWalkSFX3D(Vector3 position)   => PlaySFX3D(walkClip, position);
    public void PlayPlaceSFX3D(Vector3 position)  => PlaySFX3D(placeClip, position);
    public void PlayDeleteSFX3D(Vector3 position) => PlaySFX3D(deleteClip, position);

    private Vector3 GetListenerPosition()
    {
        // Ganti FindObjectOfType -> FindFirstObjectByType (menghilangkan warning CS0618)
        AudioListener listener = FindFirstObjectByType<AudioListener>();
        return listener != null ? listener.transform.position : Vector3.zero;
    }

    private void PlaySFX3D(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        // Hormati toggle SFX (ToggleAudio pakai key "SFX_ON")
        bool isSfxOn = PlayerPrefs.GetInt("SFX_ON", 1) == 1;
        if (!isSfxOn) return;

        // Buat GameObject sementara dengan AudioSource 3D
        GameObject tempGO = new GameObject("SFX_" + clip.name);
        tempGO.transform.position = position;

        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 1f;  
        source.minDistance = 1f;
        source.maxDistance = 25f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.playOnAwake = false;

        source.Play();
        Destroy(tempGO, clip.length);
    }
}
