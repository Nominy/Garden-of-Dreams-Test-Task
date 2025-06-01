using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameOverUI : MonoBehaviour
{
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    
    [Header("Game Over Text")]
    [SerializeField] private TextMeshProUGUI gameOverTitle;
    [SerializeField] private TextMeshProUGUI gameOverSubtitle;
    
    [Header("Game Statistics")]
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI enemiesKilledText;
    [SerializeField] private TextMeshProUGUI itemsCollectedText;
    
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button saveGameButton;
    [SerializeField] private Button quitButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float titleDelay = 0.5f;
    [SerializeField] private float statsDelay = 1f;
    [SerializeField] private float buttonsDelay = 1.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip buttonClickSound;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem gameOverParticles;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.8f);
    
    private AudioSource audioSource;
    private bool isVisible = false;
    private GameStats currentGameStats;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize canvas group if not assigned
        if (gameOverCanvasGroup == null && gameOverPanel != null)
        {
            gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (gameOverCanvasGroup == null)
            {
                gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            }
        }
    }
    
    private void Start()
    {
        SetupButtonEvents();
        HideGameOverScreen();
    }
    
    private void SetupButtonEvents()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() => { PlayButtonSound(); RestartGame(); });
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() => { PlayButtonSound(); GoToMainMenu(); });
        }
        
        if (saveGameButton != null)
        {
            saveGameButton.onClick.AddListener(() => { PlayButtonSound(); SaveGame(); });
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => { PlayButtonSound(); QuitGame(); });
        }
    }
    
    public void ShowGameOverScreen()
    {
        if (isVisible) return;
        
        isVisible = true;
        CollectGameStats();
        StartCoroutine(ShowGameOverAnimation());
    }
    
    public void HideGameOverScreen()
    {
        isVisible = false;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (gameOverCanvasGroup != null)
            gameOverCanvasGroup.alpha = 0f;
    }
    
    private IEnumerator ShowGameOverAnimation()
    {
        // Show panel immediately but invisible
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        if (gameOverCanvasGroup != null)
            gameOverCanvasGroup.alpha = 0f;
        
        // Play game over sound
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
        
        // Start particle effects
        if (gameOverParticles != null)
        {
            gameOverParticles.Play();
        }
        
        // Fade in background overlay
        if (backgroundOverlay != null)
        {
            Color startColor = backgroundOverlay.color;
            startColor.a = 0f;
            backgroundOverlay.color = startColor;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(0f, overlayColor.a, elapsedTime / fadeInDuration);
                Color currentColor = overlayColor;
                currentColor.a = alpha;
                backgroundOverlay.color = currentColor;
                yield return null;
            }
        }
        
        // Fade in the entire panel
        if (gameOverCanvasGroup != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                gameOverCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            gameOverCanvasGroup.alpha = 1f;
        }
        
        // Show title with delay
        yield return new WaitForSecondsRealtime(titleDelay);
        ShowGameOverTitle();
        
        // Show stats with delay
        yield return new WaitForSecondsRealtime(statsDelay - titleDelay);
        ShowGameStats();
        
        // Show buttons with delay
        yield return new WaitForSecondsRealtime(buttonsDelay - statsDelay);
        ShowButtons();
    }
    
    private void ShowGameOverTitle()
    {
        if (gameOverTitle != null)
        {
            gameOverTitle.gameObject.SetActive(true);
            gameOverTitle.text = "GAME OVER";
        }
        
        if (gameOverSubtitle != null)
        {
            gameOverSubtitle.gameObject.SetActive(true);
            gameOverSubtitle.text = "Better luck next time!";
        }
    }
    
    private void ShowGameStats()
    {
        if (currentGameStats != null)
        {
            if (playTimeText != null)
            {
                playTimeText.gameObject.SetActive(true);
                System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(currentGameStats.playTime);
                playTimeText.text = $"Time Played: {timeSpan:mm\\:ss}";
            }
            
            if (scoreText != null)
            {
                scoreText.gameObject.SetActive(true);
                scoreText.text = $"Score: {currentGameStats.score:N0}";
            }
            
            if (enemiesKilledText != null)
            {
                enemiesKilledText.gameObject.SetActive(true);
                enemiesKilledText.text = $"Enemies Defeated: {currentGameStats.enemiesKilled}";
            }
            
            if (itemsCollectedText != null)
            {
                itemsCollectedText.gameObject.SetActive(true);
                itemsCollectedText.text = $"Items Collected: {currentGameStats.itemsCollected}";
            }
        }
    }
    
    private void ShowButtons()
    {
        if (restartButton != null)
            restartButton.gameObject.SetActive(true);
        
        if (mainMenuButton != null)
            mainMenuButton.gameObject.SetActive(true);
        
        if (saveGameButton != null)
        {
            saveGameButton.gameObject.SetActive(true);
            // Enable save button only if we can save
            saveGameButton.interactable = SaveManager.Instance != null;
        }
        
        if (quitButton != null)
            quitButton.gameObject.SetActive(true);
    }
    
    private void CollectGameStats()
    {
        currentGameStats = new GameStats();
        
        // Get play time from GameManager
        if (GameManager.Instance != null)
        {
            currentGameStats.playTime = GameManager.Instance.GamePlayTime;
        }
        
        // Try to collect stats from various game systems
        CollectPlayerStats();
        CollectCombatStats();
        CollectItemStats();
    }
    
    private void CollectPlayerStats()
    {
        // Try to find player and get stats
        var player = FindObjectOfType<Player>();
        if (player != null)
        {
            // You can add methods to Player script to get stats
            // For now, using placeholder values
            currentGameStats.score = GetPlayerScore(player);
        }
    }
    
    private void CollectCombatStats()
    {
        // You can implement a combat stats tracker
        // For now, using placeholder values
        currentGameStats.enemiesKilled = GetEnemiesKilledCount();
    }
    
    private void CollectItemStats()
    {
        // You can implement an item collection tracker
        // For now, using placeholder values
        currentGameStats.itemsCollected = GetItemsCollectedCount();
    }
    
    // Placeholder methods - you should implement these based on your game systems
    private int GetPlayerScore(Player player)
    {
        // Implement based on your scoring system
        return Random.Range(100, 1000); // Placeholder
    }
    
    private int GetEnemiesKilledCount()
    {
        // Implement based on your combat system
        return Random.Range(5, 25); // Placeholder
    }
    
    private int GetItemsCollectedCount()
    {
        // Implement based on your inventory system
        return Random.Range(3, 15); // Placeholder
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f; // Reset time scale in case it was paused
        GameManager.Instance.RestartGame();
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Reset time scale
        GameManager.Instance.LoadMainMenu();
    }
    
    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
            
            // Provide feedback
            if (saveGameButton != null)
            {
                var originalText = saveGameButton.GetComponentInChildren<TextMeshProUGUI>();
                if (originalText != null)
                {
                    StartCoroutine(ShowSaveConfirmation(originalText));
                }
            }
        }
    }
    
    private IEnumerator ShowSaveConfirmation(TextMeshProUGUI buttonText)
    {
        string originalText = buttonText.text;
        Color originalColor = buttonText.color;  // Store original color
        
        buttonText.text = "Saved!";
        buttonText.color = Color.green;
        
        yield return new WaitForSecondsRealtime(1f);
        
        buttonText.text = originalText;
        buttonText.color = originalColor;  // Restore original color
    }
    
    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
    
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    private void OnEnable()
    {
        GameManager.OnGameOver += ShowGameOverScreen;
    }
    
    private void OnDisable()
    {
        GameManager.OnGameOver -= ShowGameOverScreen;
    }
}

[System.Serializable]
public class GameStats
{
    public float playTime;
    public int score;
    public int enemiesKilled;
    public int itemsCollected;
    public int level;
} 