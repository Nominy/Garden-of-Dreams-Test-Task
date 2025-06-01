using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    
    [Header("Main Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Load Game Panel")]
    [SerializeField] private TextMeshProUGUI saveInfoText;
    [SerializeField] private Button loadConfirmButton;
    [SerializeField] private Button deleteSaveButton;
    [SerializeField] private Button backFromLoadButton;
    
    [Header("Settings Panel")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button backFromSettingsButton;
    
    [Header("Credits Panel")]
    [SerializeField] private Button backFromCreditsButton;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI gameTitle;
    [SerializeField] private GameObject loadingIndicator;
    
    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;
    
    private AudioSource audioSource;
    private bool isLoading = false;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void Start()
    {
        InitializeMenu();
        SetupButtonEvents();
        UpdateLoadGameButton();
        LoadSettings();
    }
    
    private void InitializeMenu()
    {
        // Show main panel by default
        ShowMainPanel();
        
        // Hide loading indicator
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
        
        // Set game title
        if (gameTitle != null)
            gameTitle.text = Application.productName;
    }
    
    private void SetupButtonEvents()
    {
        // Main menu buttons
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(() => { PlayButtonSound(); StartNewGame(); });
            AddHoverSound(newGameButton);
        }
        
        if (loadGameButton != null)
        {
            loadGameButton.onClick.AddListener(() => { PlayButtonSound(); ShowLoadGamePanel(); });
            AddHoverSound(loadGameButton);
        }
        
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(() => { PlayButtonSound(); ShowSettingsPanel(); });
            AddHoverSound(settingsButton);
        }
        
        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(() => { PlayButtonSound(); ShowCreditsPanel(); });
            AddHoverSound(creditsButton);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => { PlayButtonSound(); QuitGame(); });
            AddHoverSound(quitButton);
        }
        
        // Load game panel buttons
        if (loadConfirmButton != null)
        {
            loadConfirmButton.onClick.AddListener(() => { PlayButtonSound(); LoadGame(); });
            AddHoverSound(loadConfirmButton);
        }
        
        if (deleteSaveButton != null)
        {
            deleteSaveButton.onClick.AddListener(() => { PlayButtonSound(); DeleteSave(); });
            AddHoverSound(deleteSaveButton);
        }
        
        if (backFromLoadButton != null)
        {
            backFromLoadButton.onClick.AddListener(() => { PlayButtonSound(); ShowMainPanel(); });
            AddHoverSound(backFromLoadButton);
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
            backFromSettingsButton.onClick.AddListener(() => { PlayButtonSound(); ShowMainPanel(); });
            AddHoverSound(backFromSettingsButton);
        }
        
        // Credits panel
        if (backFromCreditsButton != null)
        {
            backFromCreditsButton.onClick.AddListener(() => { PlayButtonSound(); ShowMainPanel(); });
            AddHoverSound(backFromCreditsButton);
        }
    }
    
    private void AddHoverSound(Button button)
    {
        if (button != null && buttonHoverSound != null)
        {
            var trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { PlayHoverSound(); });
            trigger.triggers.Add(entry);
        }
    }
    
    public void StartNewGame()
    {
        if (isLoading) return;
        
        StartCoroutine(StartNewGameCoroutine());
    }
    
    private IEnumerator StartNewGameCoroutine()
    {
        isLoading = true;
        ShowLoadingIndicator(true);
        
        // Small delay for UI feedback
        yield return new WaitForSeconds(0.5f);
        
        GameManager.Instance.StartNewGame();
    }
    
    public void LoadGame()
    {
        if (isLoading) return;
        
        if (GameManager.Instance.HasSaveFile)
        {
            StartCoroutine(LoadGameCoroutine());
        }
        else
        {
            Debug.LogWarning("No save file found to load!");
        }
    }
    
    private IEnumerator LoadGameCoroutine()
    {
        isLoading = true;
        ShowLoadingIndicator(true);
        
        // Small delay for UI feedback
        yield return new WaitForSeconds(0.5f);
        
        GameManager.Instance.LoadGame();
        GameManager.Instance.LoadGameScene();
    }
    
    public void DeleteSave()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
            UpdateLoadGameButton();
            UpdateSaveInfo();
            ShowMainPanel(); // Go back to main panel after deletion
        }
    }
    
    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
    
    public void ShowMainPanel()
    {
        SetActivePanel(mainPanel);
    }
    
    public void ShowLoadGamePanel()
    {
        SetActivePanel(loadGamePanel);
        UpdateSaveInfo();
    }
    
    public void ShowSettingsPanel()
    {
        SetActivePanel(settingsPanel);
    }
    
    public void ShowCreditsPanel()
    {
        SetActivePanel(creditsPanel);
    }
    
    private void SetActivePanel(GameObject targetPanel)
    {
        // Hide all panels
        if (mainPanel != null) mainPanel.SetActive(false);
        if (loadGamePanel != null) loadGamePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        
        // Show target panel
        if (targetPanel != null)
            targetPanel.SetActive(true);
    }
    
    private void UpdateLoadGameButton()
    {
        if (loadGameButton != null)
        {
            loadGameButton.interactable = GameManager.Instance.HasSaveFile;
        }
    }
    
    private void UpdateSaveInfo()
    {
        if (saveInfoText == null) return;
        
        if (SaveManager.Instance != null)
        {
            var saveInfo = SaveManager.Instance.GetSaveFileInfo();
            if (saveInfo != null)
            {
                saveInfoText.text = $"Save Date: {saveInfo.saveTime:MM/dd/yyyy HH:mm}\n" +
                                   $"Play Time: {saveInfo.GetFormattedPlayTime()}\n" +
                                   $"Scene: {saveInfo.currentScene}\n" +
                                   $"Level: {saveInfo.playerLevel}\n" +
                                   $"File Size: {saveInfo.GetFormattedFileSize()}";
                
                if (loadConfirmButton != null)
                    loadConfirmButton.interactable = true;
                
                if (deleteSaveButton != null)
                    deleteSaveButton.interactable = true;
            }
            else
            {
                saveInfoText.text = "No save file found.";
                
                if (loadConfirmButton != null)
                    loadConfirmButton.interactable = false;
                
                if (deleteSaveButton != null)
                    deleteSaveButton.interactable = false;
            }
        }
    }
    
    private void ShowLoadingIndicator(bool show)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(show);
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
    
    // Audio methods
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    private void PlayHoverSound()
    {
        if (audioSource != null && buttonHoverSound != null)
        {
            audioSource.PlayOneShot(buttonHoverSound);
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to events
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
    
    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Loading)
        {
            ShowLoadingIndicator(true);
        }
        else
        {
            ShowLoadingIndicator(false);
            isLoading = false;
        }
    }
} 