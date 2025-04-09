using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameState
{
    Idle,
    Countdown,
    Playing,
    GameOver
}

public class FishingGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float gameDuration = 30f;  // how long the player can fish
    private float timeRemaining;
    public GameState currentState = GameState.Idle;

    [Header("Score Settings")]
    public int currentScore = 0;

    [Header("UI References")]
    public GameObject startButton;      // Button to start the game
    public GameObject grabRodPrompt;    // UI panel or text that says "Grab the rod"
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI currencyText;
    public GameObject endGamePanel;     // Panel that shows after game ends
    public GameObject GameUI; 
    public TextMeshProUGUI endScoreText;
    public Button playAgainButton;
    public Button exitButton;

    private void Start()
    {
        // Initial UI states
        currentState = GameState.Idle;
        countdownText.gameObject.SetActive(false);
        grabRodPrompt.SetActive(false);
        endGamePanel.SetActive(false);
        GameUI.SetActive(false);

        // Display currency on start
        currencyText.text    = $"Coins: {CurrencyManager.GetCurrency()}";


        // Hook up button events
        startButton.GetComponent<Button>().onClick.AddListener(OnStartButtonClicked);
        playAgainButton.onClick.AddListener(OnPlayAgainButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void Update()
    {
        switch (currentState)
        {
            case GameState.Countdown:
                // Countdown handled via coroutine or a separate routine
                UpdateGameTimer();
                break;

            case GameState.Playing:
                UpdateGameTimer();
                break;

            case GameState.GameOver:
                // Nothing in particular each frame
                break;
        }
    }

    private void OnStartButtonClicked()
    {
        // Hide start button, show "Grab the rod" prompt
        startButton.SetActive(false);
        grabRodPrompt.SetActive(true);
        Debug.Log("Startbtn clicked");
        StartCountdown();

        // Wait for the user to actually grab the rod...
        // You might have some event or script that detects rod pickup
        // Then call StartCountdown() from that event.
    }

    public void OnRodGrabbed()
    {
        // Called from a script that detects the user picking up the rod
        if (currentState == GameState.Idle)
        {
            grabRodPrompt.SetActive(false);
            StartCountdown();
        }
    }

    private void StartCountdown()
    {
        currentState = GameState.Countdown;
        countdownText.gameObject.SetActive(true);
        GameUI.SetActive(true);
        StartCoroutine(DoCountdownRoutine());
        Debug.Log("Countdown started");
    }

    private System.Collections.IEnumerator DoCountdownRoutine()
    {
        // 3-2-1 countdown
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        countdownText.gameObject.SetActive(false);
        StartGame();
    }

    private void StartGame()
    {
        currentState = GameState.Playing;
        timeRemaining = gameDuration;
        UpdateUI();
         Debug.Log("Game started");
    }

    private void UpdateGameTimer()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                EndGame();
            }
        }
        UpdateUI();
    }

    public void SaveHighScore(int currentScore)
    {
        // Get the stored highscore, defaulting to 0 if not set
        int storedHighScore = PlayerPrefs.GetInt("HighScore", 0);

        // If the current score is higher, update the highscore
        if (currentScore > storedHighScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save(); // Saves the changes to disk
        }
    }


    private void EndGame()
    {
        currentState = GameState.GameOver;

        // 1) Save high score
        SaveHighScore(currentScore);

        // 2) Deposit this session’s points into the persistent currency
        CurrencyManager.ModifyCurrency(currentScore);

        // 3) Show end‑game UI
        endGamePanel.SetActive(true);
        endScoreText.text    = $"Your Score: {currentScore}";
        highScoreText.text   = $"High Score: {PlayerPrefs.GetInt("HighScore", 0)}";
        currencyText.text    = $"Coins: {CurrencyManager.GetCurrency()}";
    }

    private void UpdateUI()
    {
        timerText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();
        scoreText.text = "Score: " + currentScore;
    }

    // Called by FishingArea or other scripts to add score
    public void AddScore(int amount)
    {
        if (currentState == GameState.Playing)
        {
            currentScore += amount;
            UpdateUI();
        }
    }

    private void OnPlayAgainButtonClicked()
    {
        // Reload the scene or reset states to start a new round
        // For simplicity, we’ll just reset everything
        currentScore = 0;
        currentState = GameState.Countdown;
        endGamePanel.SetActive(false);
        startButton.SetActive(true);
        timerText.text = "";
        scoreText.text = "Score: 0";
        StartCountdown();
    }

    private void OnExitButtonClicked()
    {
        currentScore = 0;
        currentState = GameState.Idle;
        endGamePanel.SetActive(false);
        startButton.SetActive(true);
        GameUI.SetActive(false);
        timerText.text = "";
        scoreText.text = "Score: 0";
        Debug.Log("Exiting Fishing Game...");
        // Application.Quit(); // or load another scene
    }
}
