using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class ToggleAudio : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Type of Audio")]
    public bool isBGM = false;
    public bool isSFX = false;


    [Header("Player Pref Key")]
    public string bgmKey = "BGM_ON";
    public string sfxKey = "SFX_ON";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       audioSource = GetComponent<AudioSource>();
       if(!PlayerPrefs.HasKey(bgmKey))
       {
            PlayerPrefs.SetInt(bgmKey, 1);
        }
        if (!PlayerPrefs.HasKey(sfxKey))
        {
            PlayerPrefs.SetInt(sfxKey, 1);
        }

        ApplyPrefs();

    }

    // Update is called once per frame
    void Update()
    {
        ApplyPrefs();
    }

    void ApplyPrefs()
    {
        if(isBGM)
        {
            bool isBgmOn = PlayerPrefs.GetInt(bgmKey, 1) == 1;
            audioSource.mute = !isBgmOn;
        }
        if(isSFX)
        {
            bool isSfxOn = PlayerPrefs.GetInt(sfxKey, 1) == 1;
            audioSource.mute = !isSfxOn;
        }
    }
}
