using UnityEngine;
using UnityEngine.UI;

public class SoundToggleButton : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button Button;

    [Header("Sprites")]
    public Sprite OnSprite;
    public Sprite OffSprite;

    [Header("Pref Name")]
    public string PrefName;

    private bool isOn = true;

    void Start()
    {
        isOn = PlayerPrefs.GetInt(PrefName, 1) == 1;
        if(Button)
            Button.onClick.AddListener(Toggle);

        UpdateButtonVisuals();
    }

    void Toggle()
    {
        isOn = !isOn;

        PlayerPrefs.SetInt(PrefName, isOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateButtonVisuals();
    }

    void UpdateButtonVisuals()
    {
        // Change BGM button sprite
        if (Button != null && Button.image != null)
        {
            Button.image.sprite = isOn ? OnSprite : OffSprite;
        }

        
    }
}
