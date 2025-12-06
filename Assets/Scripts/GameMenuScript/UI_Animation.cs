using UnityEngine;

public class UI_Animation : MonoBehaviour
{
    private bool creditOpen = false;
    private GameObject creditPanel;

    private GameObject SettingPanel;
    private bool isSettingPanelActive = false;

    private GameObject Judul;
    private GameObject PlayButton;
    private GameObject LevelButton;
    private GameObject HowToPlayButton;
    private GameObject QuitButton;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isSettingPanelActive = false;
        SettingPanel = GameObject.Find("SettingPanel");
        if (SettingPanel != null)
            SettingPanel.SetActive(isSettingPanelActive);

        creditPanel = GameObject.Find("CreditPanel");
        if (creditPanel != null) creditPanel.SetActive(false);

        Judul = this.transform.Find("Judul").gameObject;
        PlayButton = this.transform.Find("buttons").Find("play button").gameObject;
        LevelButton = this.transform.Find("buttons").Find("Level").gameObject;
        HowToPlayButton = this.transform.Find("buttons").Find("How to play").gameObject;
        QuitButton = this.transform.Find("buttons").Find("quit").gameObject;

      

        iTween.MoveFrom(Judul, iTween.Hash("x", -2000, "time", 2f, "delay", 0f, "easeType", "easeOutExpo"));
        iTween.ScaleFrom(PlayButton, iTween.Hash("x", 0, "y", 0, "time", 2f, "delay", 1f, "easeType", "easeOutQuart"));
        iTween.ScaleFrom(LevelButton, iTween.Hash("x", 0, "y", 0, "time", 2f, "delay", 2f, "easeType", "easeOutQuart"));
        iTween.ScaleFrom(HowToPlayButton, iTween.Hash("x", 0, "y", 0, "time", 2f, "delay", 3f, "easeType", "easeOutQuart"));
        iTween.ScaleFrom(QuitButton, iTween.Hash("x", 0, "y", 0, "time", 2f, "delay", 4f, "easeType", "easeOutQuart"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Credit()
    {
        creditOpen = !creditOpen;
        if (creditOpen)
        {
            creditPanel.SetActive(true);
            iTween.MoveFrom(creditPanel, iTween.Hash("y", -1000, "time", 2f, "easeType", "easeOutExpo"));
        }
        else
        {
            iTween.MoveTo(creditPanel, iTween.Hash("y", -1000, "time", 1f, "easeType", "easeInExpo", "oncomplete", "DeactivateCreditPanel", "oncompletetarget", this.gameObject));
        }
        
    }

    void DeactivateCreditPanel()
    {
        creditPanel.SetActive(false);
    }

    public void ToggleSettingsPanel()
    {
        isSettingPanelActive = !isSettingPanelActive;
        if(isSettingPanelActive) {
            SettingPanel.SetActive(true);
            iTween.MoveFrom(SettingPanel, iTween.Hash("y", 1000, "time", 2f, "easeType", "easeOutExpo"));
        } else {
            iTween.MoveTo(SettingPanel, iTween.Hash("y", 1000, "time", 1f, "easeType", "easeInExpo", "oncomplete", "DeactivateSettingPanel", "oncompletetarget", this.gameObject));
        }
    }

    void DeactivateSettingPanel()
    {
        SettingPanel.SetActive(false);
    }

}
