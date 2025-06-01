using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause Menu Panel")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    
    [Header("Pause Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button backFromSettingsButton;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI pauseTitle;
    [SerializeField] private TextMeshProUGUI gameTimeText;
    
    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode pauseKeyAlt = KeyCode.P;
    
    [Header("Animation")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip pauseSound;
    [SerializeField] private AudioClip resumeSound;
    [SerializeField] private AudioClip buttonClickSound;
    
    private AudioSource audioSource;
    private bool isPaused = false;
    private bool isTransitioning = false;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize canvas group if not assigned
        if (pauseCanvasGroup == null && pausePanel != null)
        {
            pauseCanvasGroup = pausePanel.GetComponent<CanvasGroup>();
            if (pauseCanvasGroup == null)
            {
                pauseCanvasGroup = pausePanel.AddComponent<CanvasGroup>();
            }
        }
    }
    
    private void Start()
    {
        SetupButtonEvents();
        HidePauseMenu();
        LoadSettings();
    }
    
    private void Update()
    {
        // Handle pause input
        if (Input.GetKeyDown(pauseKey) || Input.GetKeyDown(pauseKeyAlt))
        {
            TogglePause();
        }
    }
    
    private void SetupButtonEvents()
    {
        // Pause menu buttons
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() => { PlayButtonSound(); ResumeGame(); });
        }
        
        if (saveGameButton != null)
        {
            saveGameButton.onClick.AddListener(() => { PlayButtonSound(); SaveGame(); });
        }
        
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(() => { PlayButtonSound(); ShowSettingsPanel(); });
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() => { PlayButtonSound(); GoToMainMenu(); });
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => { PlayButtonSound(); QuitGame(); });
        }
        
        // Settings panel
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }
        
        if (backFromSettingsButton != null)
        {
            backFromSettingsButton.onClick.AddListener(() => { PlayButtonSound(); ShowPausePanel(); });
        }
    }
    
    public void TogglePause()
    {
        if (isTransitioning) return;
        
        // Don't allow pausing if game is over or in main menu
        if (GameManager.Instance != null)
        {
            var currentState = GameManager.Instance.CurrentState;
            if (currentState == GameState.GameOver || currentState == GameState.MainMenu || currentState == GameState.Loading)
                return;
        }
        
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        if (isPaused || isTransitioning) return;
        
        isPaused = true;
        isTransitioning = true;
        
        // Play pause sound
        if (audioSource != null && pauseSound != null)
        {
            audioSource.PlayOneShot(pauseSound);
        }
        
        // Pause game through GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
            // Fallback if GameManager not available
            Time.timeScale = 0f;
        }
        
        UpdateGameTimeDisplay();
        StartCoroutine(ShowPauseMenuAnimation());
    }
    
    public void ResumeGame()
    {
        if (!isPaused || isTransitioning) return;
        
        isPaused = false;
        isTransitioning = true;
        
        // Play resume sound
        if (audioSource != null && resumeSound != null)
        {
            audioSource.PlayOneShot(resumeSound);
        }
        
        StartCoroutine(HidePauseMenuAnimation());
    }
    
    private IEnumerator ShowPauseMenuAnimation()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
        ShowPausePanel(); // Ensure we're on the main pause panel
        
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                pauseCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }
        
        isTransitioning = false;
    }
    
    private IEnumerator HidePauseMenuAnimation()
    {
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                pauseCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                yield return null;
            }
            
            pauseCanvasGroup.alpha = 0f;
        }
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        // Resume game through GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
            // Fallback if GameManager not available
            Time.timeScale = 1f;
        }
        
        isTransitioning = false;
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
        else
        {
            Debug.LogWarning("SaveManager not found!");
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
    
    public void GoToMainMenu()
    {
        // Resume time scale before changing scenes
        Time.timeScale = 1f;
        isPaused = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
    
    public void QuitGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
    
    public void ShowPausePanel()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        UpdateGameTimeDisplay();
        UpdateSaveButtonState();
    }
    
    public void ShowSettingsPanel()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    
    private void HidePauseMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        if (pauseCanvasGroup != null)
            pauseCanvasGroup.alpha = 0f;
    }
    
    private void UpdateGameTimeDisplay()
    {
        if (gameTimeText != null && GameManager.Instance != null)
        {
            float playTime = GameManager.Instance.GamePlayTime;
            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(playTime);
            gameTimeText.text = $"Play Time: {timeSpan:hh\\:mm\\:ss}";
        }
    }
    
    private void UpdateSaveButtonState()
    {
        if (saveGameButton != null)
        {
            saveGameButton.interactable = SaveManager.Instance != null;
        }
    }
    
    // Settings methods
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        SaveSettings();
    }
    
    private void OnFullscreenChanged(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        SaveSettings();
    }
    
    private void LoadSettings()
    {
        // Load volume
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        AudioListener.volume = volume;
        if (volumeSlider != null)
            volumeSlider.value = volume;
        
        // Load fullscreen
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = fullscreen;
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = fullscreen;
    }
    
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("Volume", AudioListener.volume);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.Save();
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
        // Subscribe to events
        GameManager.OnGamePaused += OnGamePaused;
        GameManager.OnGameResumed += OnGameResumed;
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        GameManager.OnGamePaused -= OnGamePaused;
        GameManager.OnGameResumed -= OnGameResumed;
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
    
    private void OnGamePaused()
    {
        // Handle external pause events if needed
    }
    
    private void OnGameResumed()
    {
        // Handle external resume events if needed
        if (isPaused)
        {
            isPaused = false;
            HidePauseMenu();
        }
    }
    
    private void OnGameStateChanged(GameState newState)
    {
        // Hide pause menu if game state changes to something incompatible
        if (newState == GameState.GameOver || newState == GameState.MainMenu || newState == GameState.Loading)
        {
            if (isPaused)
            {
                isPaused = false;
                HidePauseMenu();
                Time.timeScale = 1f; // Ensure time scale is reset
            }
        }
    }
    
    // Public getters
    public bool IsPaused => isPaused;
} 