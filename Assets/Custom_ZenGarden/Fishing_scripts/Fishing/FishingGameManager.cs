using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

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
    public float gameDuration = 30f;
    private float timeRemaining;
    public GameState currentState = GameState.Idle;

    [Header("Score Settings")]
    public int currentScore = 0;

    [Header("UI References")]
    public GameObject startButton;
    public GameObject startGamePanel;
    public GameObject fishingRod;
    public GameObject Bait;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI currencyText;
    public GameObject endGamePanel;
    public GameObject GameUI;
    public TextMeshProUGUI endScoreText;
    public Button playAgainButton;
    public Button exitButton;
    public AudioClip gameStartSound;
    public AudioClip gameEndSound;
    public AudioSource audioPlayer;

    [Header("References")]
    public ShopManager shopManager;

    private void Start()
    {
        // Initial UI states
        currentState = GameState.Idle;
        countdownText.gameObject.SetActive(false);
        DisableFishingRod();
        endGamePanel.SetActive(false);
        GameUI.SetActive(false);

        // Find ShopManager if not assigned
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();

        // Display currency on start
        int startingCoins = shopManager != null ? shopManager.currentCurrency : 0;
        currencyText.text = $"Coins: {startingCoins}";

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
            case GameState.Playing:
                UpdateGameTimer();
                break;
            case GameState.GameOver:
                break;
        }
    }

    private void OnStartButtonClicked()
    {
        startGamePanel.SetActive(false);
        EnableFishingRod();
        StartCountdown();
    }

    public void OnRodGrabbed()
    {
        if (currentState == GameState.Idle)
        {
            DisableFishingRod();
            StartCountdown();
        }
    }

    private void StartCountdown()
    {
        PlaySound(gameStartSound, 0.4f);
        currentState = GameState.Countdown;
        countdownText.gameObject.SetActive(true);
        GameUI.SetActive(true);
        StartCoroutine(DoCountdownRoutine());
    }

    private System.Collections.IEnumerator DoCountdownRoutine()
    {
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
        int storedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > storedHighScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save();
        }
    }

    private void EndGame()
    {
        DisableFishingRod();
        GameUI.SetActive(false);

        currentState = GameState.GameOver;
        PlaySound(gameEndSound, 0.7f);

        // 1) Save high score
        SaveHighScore(currentScore);

        // 2) Add earned coins via ShopManager
        if (shopManager != null)
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.IsMasterClient)
                    shopManager.AddCurrency(currentScore);
                else
                    shopManager.photonView.RPC("RequestAddCurrencyRPC", RpcTarget.MasterClient, currentScore);
            }
            else
            {
                shopManager.AddCurrency(currentScore);
            }

            DisableFishingRod();
        }

        // 3) Show end-game UI
        endGamePanel.SetActive(true);
        endScoreText.text = $"Your Score: {currentScore}";
        highScoreText.text = $"High Score: {PlayerPrefs.GetInt("HighScore", 0)}";

        int updatedCoins = shopManager != null ? shopManager.currentCurrency : 0;
        currencyText.text = $"Coins:\n{updatedCoins}";
    }

    public void EnableFishingRod()
	{
		fishingRod.SetActive(true);
        Bait.SetActive(true);
	}

    public void DisableFishingRod()
	{
		fishingRod.SetActive(false);
        Bait.SetActive(false);
	}

    public void UpdateCurrencyUI(int coins)
    {
        currencyText.text = $"Coins: {coins}";
    }

    private void UpdateUI()
    {
        timerText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();
        scoreText.text = "Score: " + currentScore;
    }

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
        currentScore = 0;
        currentState = GameState.Countdown;
        endGamePanel.SetActive(false);
        startButton.SetActive(true);
        EnableFishingRod();
        timerText.text = "";
        scoreText.text = "Score: 0";
        StartCountdown();
    }

    private void OnExitButtonClicked()
    {
        currentScore = 0;
        currentState = GameState.Idle;
        endGamePanel.SetActive(false);
        startGamePanel.SetActive(true);
        timerText.text = "";
        scoreText.text = "Score: 0";
        DisableFishingRod();
    }

    private void PlaySound(AudioClip clip, float volumeScale = 1.0f)
    {
        if (audioPlayer != null && clip != null)
            audioPlayer.PlayOneShot(clip, volumeScale);
    }
}
