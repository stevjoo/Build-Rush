using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelSelectorManager : MonoBehaviour
{
    public LevelListContainer levelList;

    public TMP_Text levelNameText;
    public Image levelThumbnailImage;
    public TMP_Text levelTimeText;
    public TMP_Text levelMatchText;

    public Button nextButton;
    public Button previousButton;
    public Button selectButton;
    public GameObject lockImage;

    public GameObject completeImage;

    private int currentLevelIndex = 0;

    private GameObject SettingPanel;
    private bool isSettingPanelActive = false;

    private Vector3 settingStartPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isSettingPanelActive = false;
        SettingPanel = GameObject.Find("SettingPanel");
        if (SettingPanel != null)
        {
            SettingPanel.SetActive(isSettingPanelActive);
            settingStartPos = SettingPanel.transform.localPosition;
        }

        if (levelList == null || levelList.levels.Count == 0)
        {
            Debug.LogError("Level list is not assigned or empty.");
            return;
        }

        currentLevelIndex = PlayerPrefs.GetInt("SelectedLevelIndex", 0);

        nextButton.onClick.AddListener(NextLevel);
        previousButton.onClick.AddListener(PreviousLevel);
        selectButton.onClick.AddListener(SelectLevel);

        DisplayLevel(currentLevelIndex);

    }

    void DisplayLevel(int index)
    {
        if (index < 0 || index >= levelList.levels.Count)
        {
            Debug.LogError("Level index out of range.");
            return;
        }

        LevelSelectionData level = levelList.levels[index];

        if(completeImage != null)
        {
            StopAllCoroutines();

            if (!level.isCompleted)
            {
                iTween.Stop(completeImage);
                completeImage.SetActive(false);
            }
            else
            {
                StartCoroutine(AnimateCompletionImage(completeImage));
            }
            
        }


        unlockLevel(level.levelID);
        lockImage.SetActive(level.isLocked);
        selectButton.interactable = !level.isLocked;

        levelNameText.text = $"Level {level.levelID} : {level.levelName}";
        levelMatchText.text = $"Requirement\t: {level.passingScore}%";
        levelTimeText.text = $"Time\t\t: {level.timer / 60}:{(level.timer % 60).ToString("D2")}";
        levelThumbnailImage.sprite = level.levelThumbnail;

        previousButton.interactable = (currentLevelIndex > 0);
        nextButton.interactable = (currentLevelIndex < levelList.levels.Count - 1);
    }
    
    
    public void unlockLevel(int levelID)
    {
        LevelSelectionData level = levelList.GetLevelByID(levelID);
        if (level != null)
        {
            if(level.levelID > 1 && levelList.GetLevelByID(levelID - 1).isCompleted)
            {
                level.isLocked = false;
            }
        }
    }

    public void NextLevel()
    {
        if(currentLevelIndex < levelList.levels.Count - 1)
        {
            currentLevelIndex++;
            DisplayLevel(currentLevelIndex);
        }
    }

    public void PreviousLevel()
    {
        if(currentLevelIndex > 0)
        {
            currentLevelIndex--;
            DisplayLevel(currentLevelIndex);
        }
    }

    public void SelectLevel() 
    { 
        LevelSelectionData level = levelList.levels[currentLevelIndex];
        if (level.isLocked)
        {
            Debug.Log("Level is locked.");
            return;
        }
        PlayerPrefs.SetInt("SelectedLevelID", level.levelID);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("MainMenuScene");

    }

    public void ToggleSettingsPanel()
    {
        isSettingPanelActive = !isSettingPanelActive;

        iTween.Stop(SettingPanel);

        if (isSettingPanelActive)
        {
            SettingPanel.SetActive(true);
            SettingPanel.transform.localPosition = new Vector3(settingStartPos.x, 1000, settingStartPos.z);
            iTween.MoveTo(SettingPanel, iTween.Hash("position", settingStartPos, "islocal", true, "time", 0.8f, "easeType", iTween.EaseType.easeOutExpo, "ignoretimescale", true));
        }
        else
        {
            iTween.MoveTo(SettingPanel, iTween.Hash("y", 1000, "islocal", true, "time", 0.6f, "easeType", iTween.EaseType.easeInExpo, "ignoretimescale", true, "oncomplete", "DeactivateSettingPanel", "oncompletetarget", this.gameObject));
        }
    }

    void DeactivateSettingPanel()
    {
        SettingPanel.SetActive(false);
    }

    IEnumerator AnimateCompletionImage(GameObject imageToAnimate)
    {
        iTween.Stop(imageToAnimate);
        imageToAnimate.SetActive(true);

        imageToAnimate.transform.localScale = Vector3.zero;

        yield return null;

        iTween.ScaleTo(imageToAnimate, iTween.Hash("scale", Vector3.one, "time", 1f, "delay", 0f, "easeType", iTween.EaseType.easeOutBack));
    }
}
