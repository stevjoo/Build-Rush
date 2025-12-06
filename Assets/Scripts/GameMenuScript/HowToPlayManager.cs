using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // Needed for List

public class HowToPlayManager : MonoBehaviour
{
    // === PUBLIC REFERENCES (Assign in Inspector) ===
    [Header("Page Setup")]
    [Tooltip("The UI Image component for the background that will change.")]
    public Image backgroundDisplay;

    [Tooltip("List of Sprites for each page's background. Index 0 is the first page.")]
    public List<Sprite> pageBackgrounds = new List<Sprite>();

    [Header("Navigation Buttons")]
    [Tooltip("The Button component for the 'Previous' action.")]
    public Button previousButton;

    [Tooltip("The Button component for the 'Next' action.")]
    public Button nextButton;

    // === PRIVATE VARIABLES ===
    private int _currentPageIndex = 0; // Tracks the current page

    private GameObject SettingPanel;
    private bool isSettingPanelActive = false;

    // === UNITY LIFECYCLE METHODS ===
    void Start()
    {
        isSettingPanelActive = false;
        SettingPanel = GameObject.Find("SettingPanel");
        if (SettingPanel != null)
            SettingPanel.SetActive(isSettingPanelActive);
        // 1. Initial Setup
        if (pageBackgrounds.Count == 0)
        {
            Debug.LogError("Page Backgrounds list is empty! Please assign backgrounds in the Inspector.");
            // Disable buttons if there are no pages to prevent errors
            nextButton.interactable = false;
            previousButton.interactable = false;
            return;
        }

        // 2. Set the initial background
        SetPage(_currentPageIndex);

        // 3. Set up button listeners (optional, but good practice)
        // You can also do this in the Inspector's OnClick()
        nextButton.onClick.AddListener(NextPage);
        previousButton.onClick.AddListener(PreviousPage);
    }
    public void NextPage()
    {
        // Check if we are not on the last page
        if (_currentPageIndex < pageBackgrounds.Count - 1)
        {
            // Move to the next page and update
            _currentPageIndex++;
            SetPage(_currentPageIndex);
        }
        // Button disabling/enabling is handled inside SetPage()
    }

    public void PreviousPage()
    {
        // Check if we are not on the first page
        if (_currentPageIndex > 0)
        {
            // Move to the previous page and update
            _currentPageIndex--;
            SetPage(_currentPageIndex);
        }
        // Button disabling/enabling is handled inside SetPage()
    }

    // === CORE LOGIC METHOD ===
    private void SetPage(int newIndex)
    {
        // Ensure the index is within bounds (safety check)
        if (newIndex >= 0 && newIndex < pageBackgrounds.Count)
        {
            // Update the background image
            backgroundDisplay.sprite = pageBackgrounds[newIndex];

            // Disable Previous button on the FIRST page (index 0)
            previousButton.interactable = (newIndex > 0);

            // Disable Next button on the LAST page
            nextButton.interactable = (newIndex < pageBackgrounds.Count - 1);

            // Update the current index tracker
            _currentPageIndex = newIndex;

            Debug.Log($"Displaying Page {newIndex + 1} of {pageBackgrounds.Count}");
        }
        else
        {
            Debug.LogWarning($"Attempted to set page to index {newIndex}, which is out of range.");
        }
    }

    public void ToggleSettingsPanel()
    {
        isSettingPanelActive = !isSettingPanelActive;
        if (isSettingPanelActive)
        {
            SettingPanel.SetActive(true);
            iTween.MoveFrom(SettingPanel, iTween.Hash("y", 1000, "time", 2f, "easeType", "easeOutExpo"));
        }
        else
        {
            iTween.MoveTo(SettingPanel, iTween.Hash("y", 1000, "time", 1f, "easeType", "easeInExpo", "oncomplete", "DeactivateSettingPanel", "oncompletetarget", this.gameObject));
        }
    }

    void DeactivateSettingPanel()
    {
        SettingPanel.SetActive(false);
    }
}