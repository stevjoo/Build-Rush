using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Kalau pakai UI Text

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;
    public BuildAccuracyEvaluator evaluator;

    [Header("Settings")]
    public string targetLevelFile = "target_level";
    public float previewTime = 5f; // detik target muncul sebelum hilang
    public float buildTime = 30f; // waktu maksimal build

    [Header("UI")]
    public string countdownText;


    private bool gameStarted = false;
    private int score = 0;

    void Start()
    {
        StartCoroutine(StartPreGamePhase());
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
            if (countdownText != null)
                countdownText = "Memorize: " + Mathf.Ceil(timer).ToString();
            yield return null;
            timer -= Time.deltaTime;

            Debug.Log(countdownText);
        }

        // Destroy target building jika countdown habis
        foreach (var kvp in gridManager.placedBlocks)
            Destroy(kvp.Value);


        // agar GridManager tahu block sudah hilang
        gridManager.ClearInternalDictionaries(); 


        if (countdownText != null)
            countdownText = "Go!";

        Debug.Log(countdownText);
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
            if (countdownText != null)
                countdownText = "Time Left: " + Mathf.Ceil(timer).ToString();

            // --- Hitung akurasi realtime setiap frame atau tiap detik ---
            score = Mathf.RoundToInt(evaluator.CalculateBestMatchAccuracy());

            // debug
            Debug.Log("Accuracy: " + score + "% | Time left: " + Mathf.Ceil(timer));

            yield return null;
        }

        // --- Waktu habis ---
        gameStarted = false;
        evaluator.SetGameState(false);

        if (countdownText != null)
            countdownText = "Time's up! Final Score: " + score;

        Debug.Log("Build time over! Final Accuracy: " + score + "%");
    }


}