using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI; // Kalau pakai UI Text
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;
    public BuildAccuracyEvaluator evaluator;

    [Header("Cameras")]
    public Camera playerCamera;
    public Camera ghostCamera;

    [Header("Player")]
    public GameObject playerObject;

    [Header("Settings")]
    public LevelListContainer levelListContainer;
    public int selectedLevelID = 1;
    public string targetLevelFile = "target_level";
    public float previewTime = 5f; // detik target muncul sebelum hilang
    public float buildTime = 360f; // waktu maksimal build
    public int passingScore = 70; // skor minimal untuk menang


    [Header("UI")]
    public TextMeshProUGUI countdownText; // Kalau pakai TextMeshPro
    public GameObject messagePanel; // panel untuk menampilkan pesan
    public TextMeshProUGUI messageText; // text untuk menampilkan pesan
    public TextMeshProUGUI continueText;
    public GameObject pausePanel;
    public Button pauseButton;

    private GameObject SettingPanel;
    private bool isSettingPanelActive = false;

    [Header("Level Settings")]
    public LevelSelectionData currentLevelData;

    private MovementController movementController;
    private GhostCameraController ghostCameraController;
    private bool inputLocked = false;
    public bool IsInputLocked() => inputLocked;
    private bool gameStarted = false;
    private bool isPaused = false;

    private int score = 0;

    void Start()
    {
        // Reset time scale
        Time.timeScale = 1f;

        selectedLevelID = PlayerPrefs.GetInt("SelectedLevelID", 1);
        UpdateLevelData(selectedLevelID);

        movementController = FindObjectOfType<MovementController>();
        ghostCameraController = ghostCamera.GetComponent<GhostCameraController>();
        StartCoroutine(StartPreGamePhase());
        messagePanel.SetActive(false);

        if(pausePanel != null)
            pausePanel.SetActive(false);

        isSettingPanelActive = false;
        SettingPanel = GameObject.Find("SettingPanel");
        if (SettingPanel != null)
            SettingPanel.SetActive(isSettingPanelActive);

        LockCursor();
    }

    void UpdateLevelData(int levelID)
    {
        currentLevelData = levelListContainer.GetLevelByID(levelID);
        if (currentLevelData != null)
        {
            targetLevelFile = currentLevelData.levelJson;
            buildTime = currentLevelData.timer;
            passingScore = currentLevelData.passingScore;
        }
        else
        {
            Debug.LogError($"Level with ID {levelID} not found.");
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused) PauseGame();
            else ResumeGame();
        }

        pauseButton.interactable = (isPaused == false);
    }

    private IEnumerator StartPreGamePhase()
    {
        yield return ShowMessageAndWait("Memory Time\nRemember the Building!", true);
        // Lock player Movement
        inputLocked = true;
        if (movementController != null)
            movementController.movementLocked = true;

        // hide player
        if (playerObject != null)
            playerObject.SetActive(false);

        // Switch camera ke ghost camera
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (ghostCamera != null) ghostCamera.gameObject.SetActive(true);


        // Load target level untuk preview
        gridManager.LoadLevel(targetLevelFile, new Vector3(0,1,0));

        // Load target level untuk evaluator
        evaluator.LoadLevel(targetLevelFile);


        // Countdown display
        float timer = previewTime;
        while (timer > 0f)
        {
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            yield return null;
            timer -= Time.deltaTime;
            countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        }

        countdownText.text = string.Format("00:00");

        // Destroy target building jika countdown habis
        foreach (var kvp in gridManager.placedBlocks)
            Destroy(kvp.Value);


        // agar GridManager tahu block sudah hilang
        gridManager.ClearInternalDictionaries();

        // Switch back to player camera
        if (ghostCamera != null) ghostCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);
        if (playerObject != null) playerObject.SetActive(true);


        // Unlock player movement 
        inputLocked = false;
        if (movementController != null)
            movementController.movementLocked = false;


        //disini kasih image atau tulisan "Build Now!" di UI
        yield return ShowMessageAndWait("Build Time!", true);


        gameStarted = true;

        evaluator.SetGameState(true);


        // --- 4️ Mulai fase build ---
        StartCoroutine(StartBuildPhase());

    }


    private IEnumerator StartBuildPhase()
    {
        float timer = buildTime;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            // --- Update countdown UI ---
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // --- Hitung akurasi realtime setiap frame atau tiap detik ---
            score = Mathf.RoundToInt(evaluator.CalculateBestMatchAccuracy());

            // debug
            Debug.Log("Accuracy: " + score );

            yield return null;
        }

        countdownText.text = string.Format("00:00");

        // --- Waktu habis ---
        gameStarted = false;
        evaluator.SetGameState(false);

        // -- times up message --
        yield return ShowMessageAndWait("Build Time's Up!", true);
        // Tampilkan skor akhir
        string finalmsg = (score >= passingScore) ? "You Win!" : "Try Again!";
        yield return ShowMessageAndWait(finalmsg+"\nFinal Accuracy: " + score + "%", false);


        Debug.Log("Build time over! Final Accuracy: " + score + "%");

        if (score >= passingScore)
        {
            currentLevelData.isCompleted = true;
        }

        SceneManager.LoadScene("MainMenuScene");
    }

    private IEnumerator ShowMessageAndWait(string message, bool showClickMsg=true)
    { 
        yield return null; // wait one frame to avoid UI glitches

        inputLocked = true;
        if (movementController != null)
            movementController.movementLocked = true;

        Time.timeScale = 0f;
        UnlockCursor();

        messagePanel.SetActive(true);
        //iTween.ScaleFrom(messagePanel, iTween.Hash("x", 0, "y", 0, "time", 1f, "delay", 0f, "easeType", "easeOutQuart"));
        messageText.text = message;
        continueText.gameObject.SetActive(showClickMsg);
        // Tunggu klik/tap user
        bool waitingForInput = true;
        while (waitingForInput)
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                waitingForInput = false;
            }
            yield return null;
        }
        messagePanel.SetActive(false);

        Time.timeScale = 1f;
        LockCursor();

        inputLocked = false;
        if (movementController != null)
            movementController.movementLocked = false;

        

    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        //iTween.MoveFrom(pausePanel, iTween.Hash("y", 1000, "time", 2f, "easeType", "easeOutExpo"));
        Time.timeScale = 0f;
        inputLocked = true;
        if(movementController != null)
            movementController.movementLocked = true;
        ghostCameraController.movementLocked = true;

        UnlockCursor();
    }

    public void ResumeGame()
    {
        isPaused = false;
        //iTween.MoveTo(SettingPanel, iTween.Hash("y", 1000, "time", 1f, "easeType", "easeInExpo", "oncomplete", "ResumeGameSetting", "oncompletetarget", GameObject.Find("Canvas").gameObject));
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        inputLocked = false;
        if (movementController != null)
            movementController.movementLocked = false;
        ghostCameraController.movementLocked = false;

        LockCursor();

    }

    public void ToggleSettingsPanel()
    {
        isSettingPanelActive = !isSettingPanelActive;
        if (isSettingPanelActive)
        {
            SettingPanel.SetActive(true);
            //iTween.MoveFrom(SettingPanel, iTween.Hash("y", 1000, "time", 2f, "easeType", "easeOutExpo"));
        }
        else
        {
            SettingPanel.SetActive(false);
            //iTween.MoveTo(SettingPanel, iTween.Hash("y", 1000, "time", 1f, "easeType", "easeInExpo", "oncomplete", "DeactivateSettingPanel", "oncompletetarget", GameObject.Find("Canvas").gameObject));
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}