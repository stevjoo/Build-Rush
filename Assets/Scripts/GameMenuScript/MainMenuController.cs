using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public LevelListContainer levelListContainer;

    public TMP_Text LevelButtonText;
    public Image levelThumbnail;

    public
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int selectedLevelID = PlayerPrefs.GetInt("SelectedLevelID", 1);

        UpdateLevel(selectedLevelID);

    }

    public void UpdateLevel(int levelID)
    {
        LevelSelectionData levelData = levelListContainer.GetLevelByID(levelID);
        if (levelData != null)
        {
            LevelButtonText.text = $"Level: {levelData.levelID}";
            levelThumbnail.sprite = levelData.levelThumbnail;
            PlayerPrefs.SetInt("SelectedLevelID", levelID);
        }
        else
        {
            Debug.LogError($"Level with ID {levelID} not found.");
        }
    }

    public void StartGame()
    {
        int selectedLevelID = PlayerPrefs.GetInt("SelectedLevelID", 1);
        LevelSelectionData levelData = levelListContainer.GetLevelByID(selectedLevelID);
        if (levelData != null)
        {
            //sementara ku masukin ke builder scene duluwh
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.LogError($"Level with ID {selectedLevelID} not found.");
        }
    }

   
}
