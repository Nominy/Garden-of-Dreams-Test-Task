using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central manager for saving and loading game data using JSON
/// </summary>
public class SaveManager : MonoBehaviour
{
    [Header("Save Settings")]
    [SerializeField] private string saveFileName = "save.json";
    [SerializeField] private bool enableDebugLogs = true;
    
    private static SaveManager instance;
    public static SaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SaveManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Events
    public static System.Action OnSaveCompleted;
    public static System.Action OnLoadCompleted;
    public static System.Action<string> OnSaveError;
    public static System.Action<string> OnLoadError;
    
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);
    private float gameStartTime;
    
    // Current game settings - loaded at Awake, updated by UI
    private float currentVolume = 1.0f;
    private bool currentFullscreen = true;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        gameStartTime = Time.time;

        // Load settings on awake, or use defaults if no save file
        LoadGlobalSettings();
    }

    private void LoadGlobalSettings()
    {
        if (HasSaveFile())
        {
            try
            {
                string jsonData = File.ReadAllText(SaveFilePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
                currentVolume = saveData.settingsVolume;
                currentFullscreen = saveData.settingsIsFullscreen;

                AudioListener.volume = currentVolume;
                Screen.fullScreen = currentFullscreen;

                if (enableDebugLogs)
                    Debug.Log($"Global settings loaded: Volume={currentVolume}, Fullscreen={currentFullscreen}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load global settings from save file: {e.Message}. Using defaults.");
                // Apply defaults if loading fails
                ApplyDefaultSettings();
            }
        }
        else
        {
            // Apply defaults if no save file
            ApplyDefaultSettings();
            if (enableDebugLogs)
                Debug.Log("No save file found. Applied default global settings.");
        }
    }

    private void ApplyDefaultSettings()
    {
        currentVolume = 1.0f; // Default volume
        currentFullscreen = true; // Default fullscreen state (can be platform dependent)
        AudioListener.volume = currentVolume;
        Screen.fullScreen = currentFullscreen;
    }

    public float GetCurrentVolume() => currentVolume;
    public bool IsCurrentFullscreen() => currentFullscreen;

    public void UpdateAndSaveSettings(float volume, bool fullscreen)
    {
        currentVolume = volume;
        currentFullscreen = fullscreen;

        AudioListener.volume = currentVolume;
        Screen.fullScreen = currentFullscreen;

        // Save all game data, including these new settings
        // This mimics PlayerPrefs.Save() immediate behavior
        SaveData saveData = HasSaveFile() ? JsonUtility.FromJson<SaveData>(File.ReadAllText(SaveFilePath)) : new SaveData();
        
        // Update settings in the existing or new SaveData object
        saveData.settingsVolume = currentVolume;
        saveData.settingsIsFullscreen = currentFullscreen;

        // If it's a new save data (no file existed), populate other essential fields before saving
        if (!HasSaveFile())
        {
            saveData.currentScene = SceneManager.GetActiveScene().name;
            saveData.timePlayed = Time.time - gameStartTime; // Or 0 if preferred for a settings-only save
            // Note: This save won't include ISaveable object data unless SaveGame() is explicitly called
        }

        try
        {
            string jsonData = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SaveFilePath, jsonData);
            if (enableDebugLogs)
                Debug.Log($"Settings updated and game data saved. Volume: {currentVolume}, Fullscreen: {currentFullscreen}");
        }
        catch (Exception e)
        { 
            Debug.LogError($"Failed to save settings: {e.Message}");
            OnSaveError?.Invoke($"Failed to save settings: {e.Message}");
        }
    }
    
    /// <summary>
    /// Save the current game state
    /// </summary>
    public void SaveGame()
    {
        try
        {
            SaveData saveData = CollectSaveData();
            // Ensure current settings are part of this save operation
            saveData.settingsVolume = currentVolume;
            saveData.settingsIsFullscreen = currentFullscreen;

            string jsonData = JsonUtility.ToJson(saveData, true);
            
            File.WriteAllText(SaveFilePath, jsonData);
            
            if (enableDebugLogs)
                Debug.Log($"Game saved successfully to: {SaveFilePath}");
            
            OnSaveCompleted?.Invoke();
        }
        catch (Exception e)
        {
            string errorMsg = $"Failed to save game: {e.Message}\n{e.StackTrace}";
            Debug.LogError(errorMsg);
            OnSaveError?.Invoke(errorMsg);
        }
    }
    
    /// <summary>
    /// Load the game state from file
    /// </summary>
    public void LoadGame()
    {
        try
        {
            if (!HasSaveFile())
            {
                Debug.LogWarning("No save file found to load.");
                OnLoadError?.Invoke("No save file found.");
                return;
            }
            
            string jsonData = File.ReadAllText(SaveFilePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
            
            // Apply loaded settings first
            currentVolume = saveData.settingsVolume;
            currentFullscreen = saveData.settingsIsFullscreen;
            AudioListener.volume = currentVolume;
            Screen.fullScreen = currentFullscreen;
            if(enableDebugLogs)
                Debug.Log($"Settings applied from loaded game: Volume={currentVolume}, Fullscreen={currentFullscreen}");

            ApplySaveData(saveData);
            
            if (enableDebugLogs)
                Debug.Log("Game loaded successfully!");
            
            OnLoadCompleted?.Invoke();
        }
        catch (Exception e)
        {
            string errorMsg = $"Failed to load game: {e.Message}\n{e.StackTrace}";
            Debug.LogError(errorMsg);
            OnLoadError?.Invoke(errorMsg);
        }
    }
    
    /// <summary>
    /// Check if a save file exists
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(SaveFilePath);
    }
    
    /// <summary>
    /// Delete the save file
    /// </summary>
    public void DeleteSave()
    {
        try
        {
            if (HasSaveFile())
            {
                File.Delete(SaveFilePath);
                if (enableDebugLogs)
                    Debug.Log("Save file deleted successfully.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
        }
    }
    
    /// <summary>
    /// Get save file info for UI display
    /// </summary>
    public SaveFileInfo GetSaveFileInfo()
    {
        if (!HasSaveFile())
            return null;
        
        try
        {
            string jsonData = File.ReadAllText(SaveFilePath);
            SaveData tempData = JsonUtility.FromJson<SaveData>(jsonData);

            return new SaveFileInfo
            {
                saveTime = tempData.saveTime,
                timePlayed = tempData.timePlayed,
                currentScene = tempData.currentScene,
                saveVersion = tempData.saveVersion,
                playerLevel = GetPlayerLevelFromDetailed(tempData.playerDetailedData),
                fileSize = new FileInfo(SaveFilePath).Length
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read save file info: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Collect all save data from the current game state
    /// </summary>
    private SaveData CollectSaveData()
    {
        SaveData saveData = new SaveData();
        
        // Basic game info
        saveData.currentScene = SceneManager.GetActiveScene().name;
        saveData.timePlayed = Time.time - gameStartTime;
        
        // Add current settings to the save data
        saveData.settingsVolume = currentVolume;
        saveData.settingsIsFullscreen = currentFullscreen;
        
        // Collect data from all ISaveable objects
        var saveableObjects = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
        
        foreach (var saveable in saveableObjects)
        {
            try
            {
                object data = saveable.GetSaveData();
                if (data == null)
                {
                    Debug.LogWarning($"ISaveable object {saveable.GetType().Name} (ID: {saveable.GetSaveId()}) returned null data.");
                    continue;
                }

                if (data is PlayerSaveDataDetailed pData)
                {
                    saveData.playerDetailedData = pData;
                    if (enableDebugLogs)
                    {
                        Debug.Log($"Collected Player data for: {saveable.GetSaveId()}");
                        Debug.Log($"Player inventory has {pData.inventoryItems?.Count ?? 0} items");
                        if (pData.inventoryItems != null)
                        {
                            int nonNullItems = 0;
                            foreach (var item in pData.inventoryItems)
                            {
                                if (item != null) nonNullItems++;
                            }
                            Debug.Log($"Player inventory has {nonNullItems} non-null items");
                        }
                    }
                }
                else if (data is BasicEnemySaveData eData)
                {
                    saveData.enemySaveDataList.Add(eData);
                    if (enableDebugLogs)
                        Debug.Log($"Collected BasicEnemy data for: {saveable.GetSaveId()}");
                }
                else if (data is ItemPickupSaveDataDetailed iData)
                {
                    saveData.itemPickupSaveDataList.Add(iData);
                    if (enableDebugLogs)
                        Debug.Log($"Collected ItemPickup data for: {saveable.GetSaveId()}");
                }
                else
                {
                    Debug.LogWarning($"Unhandled ISaveable data type: {data.GetType().Name} from {saveable.GetType().Name} (ID: {saveable.GetSaveId()})");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error collecting save data from {saveable.GetType().Name} (ID: {saveable.GetSaveId()}): {e.Message}\n{e.StackTrace}");
            }
        }
        
        return saveData;
    }
    
    /// <summary>
    /// Apply loaded save data to the game world
    /// </summary>
    private void ApplySaveData(SaveData saveData)
    {
        // Load scene if different
        if (!string.IsNullOrEmpty(saveData.currentScene) && 
            saveData.currentScene != SceneManager.GetActiveScene().name)
        {
            // Store save data temporarily and load after scene change
            StartCoroutine(LoadSceneAndApplyData(saveData));
            return;
        }
        
        // Clear and recreate entities from save data
        ClearAndRecreateEntities(saveData);
        
        // Apply player data (player should already exist)
        ApplyPlayerDataFromDetailed(saveData.playerDetailedData);
    }
    
    /// <summary>
    /// Clear existing saveable entities and recreate them from save data
    /// </summary>
    private void ClearAndRecreateEntities(SaveData saveData)
    {
        // Clear existing enemies (except player)
        var existingEnemies = FindObjectsOfType<BasicEnemy>();
        foreach (var enemy in existingEnemies)
        {
            DestroyImmediate(enemy.gameObject);
        }
        
        // Clear existing item pickups
        var existingItems = FindObjectsOfType<ItemPickup>();
        foreach (var item in existingItems)
        {
            DestroyImmediate(item.gameObject);
        }
        
        // Recreate enemies from save data
        if (saveData.enemySaveDataList != null)
        {
            foreach (var enemyData in saveData.enemySaveDataList)
            {
                RecreateEnemy(enemyData);
            }
        }
        
        // Recreate item pickups from save data
        if (saveData.itemPickupSaveDataList != null)
        {
            foreach (var itemData in saveData.itemPickupSaveDataList)
            {
                if (!itemData.isPickedUp) // Only recreate items that weren't picked up
                {
                    RecreateItemPickup(itemData);
                }
            }
        }
    }
    
    /// <summary>
    /// Recreate an enemy from save data
    /// </summary>
    private void RecreateEnemy(BasicEnemySaveData enemyData)
    {
        GameObject enemyPrefab = Resources.Load<GameObject>("Prefabs/Enemy");
        if (enemyPrefab == null)
        {
            var templateEnemy = FindObjectOfType<BasicEnemy>(); // Check for existing enemies to use as template
            if (templateEnemy != null && templateEnemy.gameObject.name != "BasicEnemy_Recreated") // Avoid self-templating if one was already recreated poorly
            {
                 enemyPrefab = templateEnemy.gameObject;
                 if(enableDebugLogs) Debug.Log("Using an existing BasicEnemy as a template for recreation.");
            }
        }
        
        GameObject enemyObj;
        BasicEnemy enemyComponent = null;

        if (enemyPrefab != null)
        {
            enemyObj = Instantiate(enemyPrefab, enemyData.position, Quaternion.identity);
            enemyComponent = enemyObj.GetComponent<BasicEnemy>();
            if (enemyComponent == null) // If prefab didn't have BasicEnemy, add it.
            {
                Debug.LogWarning($"Prefab {enemyPrefab.name} did not have BasicEnemy component. Adding one.");
                enemyComponent = enemyObj.AddComponent<BasicEnemy>();
            }
        }
        else
        {
            if(enableDebugLogs) Debug.LogWarning("No BasicEnemy prefab or template found. Creating from scratch.");
            enemyObj = new GameObject("BasicEnemy_Recreated");
            enemyObj.transform.position = enemyData.position;
            
            // Add components one by one
            var sr = enemyObj.AddComponent<SpriteRenderer>();
            var col = enemyObj.AddComponent<CapsuleCollider2D>();
            var newRb = enemyObj.AddComponent<Rigidbody2D>(); // Explicitly add Rigidbody2D
            
            // Configure components
            newRb.gravityScale = 1f;
            col.size = Vector2.one;
            col.isTrigger = false;
            var defaultSprite = Resources.Load<Sprite>("Plot");
            if(defaultSprite) sr.sprite = defaultSprite;
            else if(enableDebugLogs) Debug.LogWarning("DefaultEnemy sprite not found in Resources.");

            // Now add BasicEnemy component AFTER others are definitely there.
            enemyComponent = enemyObj.AddComponent<BasicEnemy>(); 
        }
        
        if (enemyComponent != null)
        {
            // At this point, BasicEnemy.Awake() should have run and cached its Rigidbody2D.
            // Call LoadSaveData to apply the saved state.
            enemyComponent.LoadSaveData(enemyData); 
        }
        else
        {
            Debug.LogError($"CRITICAL: Failed to get or add BasicEnemy component when recreating enemy at {enemyData.position}. Object will be destroyed.");
            if (enemyObj != null) DestroyImmediate(enemyObj);
            return;
        }

        // Final verification step for Rigidbody2D after everything
        if (enemyComponent.GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError($"POST-INIT CRITICAL ERROR: BasicEnemy_Recreated (ID: {enemyComponent.GetSaveId()}) at {enemyData.position} STILL lacks a Rigidbody2D after LoadSaveData. This should not happen.");
        }
        else if (enableDebugLogs) 
        {
             Debug.Log($"Successfully Recreated BasicEnemy (ID: {enemyComponent.GetSaveId()}) at {enemyData.position} with Rigidbody2D.");
        }
    }
    
    /// <summary>
    /// Recreate an item pickup from save data
    /// </summary>
    private void RecreateItemPickup(ItemPickupSaveDataDetailed itemData)
    {
        // Create item pickup GameObject
        GameObject itemObj = new GameObject($"ItemPickup_{itemData.itemData?.itemName ?? "Unknown"}");
        itemObj.transform.position = itemData.position;
        
        // Add necessary components
        var spriteRenderer = itemObj.AddComponent<SpriteRenderer>();
        var collider = itemObj.AddComponent<CircleCollider2D>();
        var pickup = itemObj.AddComponent<ItemPickup>();
        
        collider.isTrigger = true;
        collider.radius = 0.5f;
        
        // Set up the pickup with the saved item data
        if (itemData.itemData != null)
        {
            var inventoryItem = itemData.itemData.ToInventoryItem();
            pickup.SetupItem(inventoryItem);
        }
        
        // Apply the full save data to restore the pickup's state
        pickup.LoadSaveData(itemData);
        
        if (enableDebugLogs)
            Debug.Log($"Recreated item: {itemData.itemData?.itemName} at {itemData.position}");
    }
    
    /// <summary>
    /// Apply player data
    /// </summary>
    private void ApplyPlayerDataFromDetailed(PlayerSaveDataDetailed detailedData)
    {
        if (detailedData == null) 
        {
            Debug.LogWarning("No detailed player data to apply.");
            return;
        }
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.LoadSaveData(detailedData);
            if (enableDebugLogs)
                Debug.Log($"Applied Player data (ID: {player.GetSaveId()})");
        }
        else
        {
            Debug.LogError("Player object not found in scene to apply save data.");
        }
    }
    
    /// <summary>
    /// Load scene and then apply save data
    /// </summary>
    private System.Collections.IEnumerator LoadSceneAndApplyData(SaveData saveData)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(saveData.currentScene);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Wait a frame for scene to fully initialize
        yield return null;
        
        // Apply save data to the new scene
        ApplySaveData(saveData);
    }
    
    /// <summary>
    /// Get player level for save file info (placeholder implementation)
    /// </summary>
    private int GetPlayerLevelFromDetailed(PlayerSaveDataDetailed detailedData)
    {
        if (detailedData == null) return 1; 
        return 1; // Placeholder
    }
}

/// <summary>
/// Information about a save file for UI display
/// </summary>
[System.Serializable]
public class SaveFileInfo
{
    public DateTime saveTime;
    public float timePlayed;
    public string currentScene;
    public string saveVersion;
    public int playerLevel;
    public long fileSize;
    
    public string GetFormattedPlayTime() => TimeSpan.FromSeconds(timePlayed).ToString(@"hh\:mm\:ss");
    
    public string GetFormattedFileSize()
    {
        if (fileSize < 1024)
            return $"{fileSize} B";
        else if (fileSize < 1024 * 1024)
            return $"{fileSize / 1024.0:F1} KB";
        else
            return $"{fileSize / (1024.0 * 1024.0):F1} MB";
    }
} 