using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Loading
}

public class GameManager : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "SampleScene";
    
    [Header("Game Settings")]
    [SerializeField] private bool autoSaveOnExit = true;
    [SerializeField] private float autoSaveInterval = 30f; // Auto-save every 30 seconds during gameplay
    
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    [Header("Current Game State")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    
    // Events
    public static System.Action<GameState> OnGameStateChanged;
    public static System.Action OnGameStarted;
    public static System.Action OnGameOver;
    public static System.Action OnGamePaused;
    public static System.Action OnGameResumed;
    
    // Game timing
    private float gameStartTime;
    private float gamePlayTime;
    private Coroutine autoSaveCoroutine;
    
    // Game Over conditions
    public bool IsGameOver { get; private set; }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Subscribe to scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Handle application events
        Application.focusChanged += OnApplicationFocus;
        Application.wantsToQuit += OnApplicationWantsToQuit;
    }
    
    private void Start()
    {
        // Initialize based on current scene
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == gameSceneName)
        {
            StartGame();
        }
        else
        {
            SetGameState(GameState.MainMenu);
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            // Game scene loaded
            if (currentState == GameState.Loading)
            {
                StartGame();
            }
        }
        else if (scene.name == mainMenuSceneName)
        {
            SetGameState(GameState.MainMenu);
        }
    }
    
    public void StartNewGame()
    {
        IsGameOver = false;
        LoadGameScene();
    }
    
    public void LoadGameScene()
    {
        SetGameState(GameState.Loading);
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void LoadMainMenu()
    {
        SetGameState(GameState.Loading);
        
        // Stop auto-save when leaving game
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
        }
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    public void StartGame()
    {
        gameStartTime = Time.time;
        gamePlayTime = 0f;
        IsGameOver = false;
        
        SetGameState(GameState.Playing);
        OnGameStarted?.Invoke();
        
        // Start auto-save coroutine
        if (autoSaveInterval > 0)
        {
            autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
        }
        
        Debug.Log("Game Started!");
    }
    
    public void GameOver()
    {
        if (IsGameOver) return; // Prevent multiple calls
        
        IsGameOver = true;
        SetGameState(GameState.GameOver);
        OnGameOver?.Invoke();
        
        // Stop auto-save
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
        }
        
        Debug.Log("Game Over!");
    }
    
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            Time.timeScale = 0f;
            SetGameState(GameState.Paused);
            OnGamePaused?.Invoke();
        }
    }
    
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            Time.timeScale = 1f;
            SetGameState(GameState.Playing);
            OnGameResumed?.Invoke();
        }
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        IsGameOver = false;
        LoadGameScene();
    }
    
    public void QuitGame()
    {
        if (autoSaveOnExit && currentState == GameState.Playing)
        {
            SaveGame();
        }
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
    }
    
    public void LoadGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveFile())
        {
            SaveManager.Instance.LoadGame();
        }
    }
    
    private void SetGameState(GameState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);
            Debug.Log($"Game State Changed: {newState}");
        }
    }
    
    private IEnumerator AutoSaveCoroutine()
    {
        while (currentState == GameState.Playing)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            
            if (currentState == GameState.Playing && !IsGameOver)
            {
                SaveGame();
                Debug.Log("Auto-save completed");
            }
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && autoSaveOnExit && currentState == GameState.Playing)
        {
            SaveGame();
        }
    }
    
    private bool OnApplicationWantsToQuit()
    {
        if (autoSaveOnExit && currentState == GameState.Playing)
        {
            SaveGame();
        }
        return true;
    }
    
    // Public getters
    public GameState CurrentState => currentState;
    public float GamePlayTime => gamePlayTime;
    public bool HasSaveFile => SaveManager.Instance?.HasSaveFile() ?? false;
    
    private void Update()
    {
        if (currentState == GameState.Playing && !IsGameOver)
        {
            gamePlayTime = Time.time - gameStartTime;
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Application.focusChanged -= OnApplicationFocus;
        Application.wantsToQuit -= OnApplicationWantsToQuit;
    }
} 