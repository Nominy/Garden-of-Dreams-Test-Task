using UnityEngine;

/// <summary>
/// Abstract base class for all player controllers.
/// Provides standardized initialization and communication with the main Player class.
/// </summary>
public abstract class PlayerControllerBase : MonoBehaviour
{
    [Header("Controller Base")]
    [SerializeField] protected bool enableDebugLogs = false;
    
    protected Player player;
    protected bool isInitialized = false;
    
    /// <summary>
    /// Initialize this controller with reference to main Player instance.
    /// Called by Player during its Awake/Start phase.
    /// </summary>
    /// <param name="playerInstance">Reference to the main Player component</param>
    public virtual void Initialize(Player playerInstance)
    {
        if (isInitialized)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"{GetType().Name} is already initialized!");
            return;
        }
        
        player = playerInstance;
        
        if (player == null)
        {
            Debug.LogError($"{GetType().Name}: Player reference is null during initialization!");
            return;
        }
        
        OnInitialize();
        isInitialized = true;
        
        if (enableDebugLogs)
            Debug.Log($"{GetType().Name} initialized successfully");
    }
    
    /// <summary>
    /// Override this method to implement controller-specific initialization logic.
    /// Called after player reference is set.
    /// </summary>
    protected virtual void OnInitialize() { }
    
    /// <summary>
    /// Override this method to implement controller-specific cleanup logic.
    /// Called when the controller is being destroyed.
    /// </summary>
    protected virtual void OnCleanup() { }
    
    /// <summary>
    /// Check if this controller is properly initialized.
    /// </summary>
    protected bool IsReady => isInitialized && player != null && player.IsAlive;
    
    /// <summary>
    /// Get the main Player reference (for derived classes).
    /// </summary>
    protected Player Player => player;
    
    void OnDestroy()
    {
        OnCleanup();
    }
    
    void OnValidate()
    {
        // Ensure we don't have multiple instances of the same controller type
        var controllers = GetComponents<PlayerControllerBase>();
        var sameTypeControllers = 0;
        
        foreach (var controller in controllers)
        {
            if (controller.GetType() == this.GetType())
                sameTypeControllers++;
        }
        
        if (sameTypeControllers > 1)
        {
            Debug.LogWarning($"Multiple {GetType().Name} components found! Only one should exist per player.");
        }
    }
} 