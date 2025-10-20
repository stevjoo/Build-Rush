using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    bool musicOn = true;
    public Button soundbutton;
    public Sprite SoundOn;
    public Sprite SoundOff;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void GoToLevelSelect()
    {
        SceneManager.LoadScene("LevelSelectionScene");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void ToggleMusic()
    {
        if (musicOn) {
            soundbutton.GetComponent<Image>().sprite = SoundOn;
        } else {
            soundbutton.GetComponent<Image>().sprite = SoundOff;
        }
        musicOn = !musicOn;
        if (musicOn)
        {
            AudioListener.volume = 1f;
        }
        else
        {
            AudioListener.volume = 0f;
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            Debug.Log("Quit Game");
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void HowToPlay()
    {
        SceneManager.LoadScene("HowToPlayScene");
    }

    public void Credit()
    {
        SceneManager.LoadScene("CreditScene");
    }
}
