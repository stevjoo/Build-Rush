using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectorManager : MonoBehaviour
{
    public LevelListContainer levelList;

    public TMP_Text levelIDText;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            completeImage.SetActive(level.isCompleted);
        }


            unlockLevel(level.levelID);
        lockImage.SetActive(level.isLocked);
        selectButton.interactable = !level.isLocked;

        levelIDText.text = $"Level {level.levelID}";
        levelMatchText.text = $"Match\t: {level.matchCount}%";
        levelTimeText.text = $"Time\t: {level.timer/60}:{(level.timer%60).ToString("D2")}";
        levelNameText.text = level.levelName;
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
}
