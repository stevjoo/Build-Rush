using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button bgmButton;

    [Header("Sprites")]
    public Sprite bgmOnSprite;
    public Sprite bgmOffSprite;

    [Header("Audio Sources")]
    public AudioSource bgmSource;

    private bool isBgmOn = true;

    void Start()
    {
        if (bgmButton != null) bgmButton.onClick.AddListener(ToggleBGM);

        UpdateButtonVisuals();
    }

    void ToggleBGM()
    {
        isBgmOn = !isBgmOn;

        if (bgmSource != null)
        {
            bgmSource.enabled = isBgmOn;
            if (isBgmOn) bgmSource.Play();
            else bgmSource.Stop();
        }

        UpdateButtonVisuals();
    }

    void UpdateButtonVisuals()
    {
        // Change BGM button sprite
        if (bgmButton != null && bgmButton.image != null)
        {
            bgmButton.image.sprite = isBgmOn ? bgmOnSprite : bgmOffSprite;
        }

        
    }
}
