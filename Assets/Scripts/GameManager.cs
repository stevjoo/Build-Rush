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

    [Header("Settings")]
    public string targetLevelFile = "target_level";
    public float previewTime = 5f; // detik target muncul sebelum hilang
    public float buildTime = 360f; // waktu maksimal build

    [Header("UI")]
    public TextMeshProUGUI countdownText; // Kalau pakai TextMeshPro
    public GameObject messagePanel; // panel untuk menampilkan pesan
    public TextMeshProUGUI messageText; // text untuk menampilkan pesan
    public TextMeshProUGUI continueText;
    public GameObject pausePanel;
    public Button pauseButton;

    private MovementController movementController;
    private bool inputLocked = false;
    public bool IsInputLocked() => inputLocked;
    private bool gameStarted = false;
    private bool isPaused = false;

    private int score = 0;

    void Start()
    {
        movementController = FindObjectOfType<MovementController>();
        StartCoroutine(StartPreGamePhase());
        messagePanel.SetActive(false);
        pausePanel.SetActive(false);
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

        // Destroy target building jika countdown habis
        foreach (var kvp in gridManager.placedBlocks)
            Destroy(kvp.Value);


        // agar GridManager tahu block sudah hilang
        gridManager.ClearInternalDictionaries();


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

        // --- Waktu habis ---
        gameStarted = false;
        evaluator.SetGameState(false);

        // -- times up message --
        yield return ShowMessageAndWait("Build Time's Up!", true);
        // Tampilkan skor akhir
        string finalmsg = (score >= 70) ? "You Win!" : "Try Again!";
        yield return ShowMessageAndWait(finalmsg+"\nFinal Accuracy: " + score + "%", false);


        Debug.Log("Build time over! Final Accuracy: " + score + "%");

        SceneManager.LoadScene("MainMenuScene");
    }

    private IEnumerator ShowMessageAndWait(string message, bool showClickMsg=true)
    { 
        inputLocked = true;
        movementController.movementLocked = true;
        messagePanel.SetActive(true);
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
        inputLocked = false;
        movementController.movementLocked = false;
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        inputLocked = true;
        if(movementController != null)
            movementController.movementLocked = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        inputLocked = false;
        if(movementController != null)
            movementController.movementLocked = false;
    }
}