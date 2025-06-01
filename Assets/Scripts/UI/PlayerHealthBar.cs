using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player health bar UI component that displays as a screen overlay (HUD element).
/// This is different from EnemyHealthBar which is world-space.
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    
    [Header("Colors")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    
    [Header("Animation Settings")]
    [SerializeField] private bool animateHealthChange = true;
    [SerializeField] private float animationSpeed = 2f;
    [SerializeField] private bool showDamageFlash = true;
    [SerializeField] private float flashDuration = 0.2f;
    
    [Header("Display Settings")]
    [SerializeField] private bool showHealthNumbers = true;
    [SerializeField] private TextMeshProUGUI healthText;
    
    private float maxHealth;
    private float currentHealth;
    private float targetHealth; // For smooth animation
    private Player playerReference;
    
    // Animation coroutine reference
    private Coroutine healthAnimationCoroutine;
    private Coroutine flashCoroutine;
    
    void Awake()
    {
        // Setup components if not assigned
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();
        
        if (fillImage == null && healthSlider != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        
        if (backgroundImage == null && healthSlider != null)
            backgroundImage = healthSlider.GetComponent<Image>();
        
        // Setup background color
        if (backgroundImage != null)
            backgroundImage.color = backgroundColor;
    }
    
    void Start()
    {
        // Find player reference
        FindPlayer();
    }
    
    private void FindPlayer()
    {
        if (playerReference == null)
        {
            playerReference = FindObjectOfType<Player>();
            
            if (playerReference != null)
            {
                // Subscribe to player health events
                playerReference.OnHealthChanged += OnPlayerHealthChanged;
                
                // Initialize with current player health
                SetMaxHealth(playerReference.MaxHealth);
                SetHealth(playerReference.Health);
                
                Debug.Log("PlayerHealthBar: Connected to player");
            }
            else
            {
                Debug.LogWarning("PlayerHealthBar: No Player found in scene!");
            }
        }
    }
    
    private void OnPlayerHealthChanged(float currentHealth, float maxHealth)
    {
        SetMaxHealth(maxHealth);
        SetHealth(currentHealth);
    }
    
    public void SetMaxHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
        }
        
        UpdateHealthDisplay();
    }
    
    public void SetHealth(float health)
    {
        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        targetHealth = currentHealth;
        
        // Show damage flash if health decreased
        if (showDamageFlash && health < previousHealth)
        {
            TriggerDamageFlash();
        }
        
        // Animate health change or set immediately
        if (animateHealthChange && Application.isPlaying)
        {
            AnimateHealthChange();
        }
        else
        {
            UpdateHealthSlider(currentHealth);
        }
        
        UpdateHealthColor();
        UpdateHealthDisplay();
    }
    
    private void AnimateHealthChange()
    {
        // Stop any existing animation
        if (healthAnimationCoroutine != null)
        {
            StopCoroutine(healthAnimationCoroutine);
        }
        
        healthAnimationCoroutine = StartCoroutine(AnimateHealthCoroutine());
    }
    
    private System.Collections.IEnumerator AnimateHealthCoroutine()
    {
        float startHealth = healthSlider != null ? healthSlider.value : currentHealth;
        float elapsedTime = 0f;
        
        while (Mathf.Abs(startHealth - targetHealth) > 0.1f)
        {
            elapsedTime += Time.deltaTime;
            float lerpProgress = elapsedTime * animationSpeed;
            
            float animatedHealth = Mathf.Lerp(startHealth, targetHealth, lerpProgress);
            UpdateHealthSlider(animatedHealth);
            
            yield return null;
            
            if (lerpProgress >= 1f)
                break;
        }
        
        // Ensure we end at the exact target
        UpdateHealthSlider(targetHealth);
        healthAnimationCoroutine = null;
    }
    
    private void UpdateHealthSlider(float health)
    {
        if (healthSlider != null)
        {
            healthSlider.value = health;
        }
    }
    
    private void UpdateHealthColor()
    {
        if (fillImage == null) return;
        
        float healthPercent = currentHealth / maxHealth;
        
        if (healthPercent > 0.6f)
        {
            fillImage.color = healthyColor;
        }
        else if (healthPercent > 0.3f)
        {
            fillImage.color = damagedColor;
        }
        else
        {
            fillImage.color = criticalColor;
        }
    }
    
    private void UpdateHealthDisplay()
    {
        if (showHealthNumbers && healthText != null)
        {
            healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
        }
    }
    
    private void TriggerDamageFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        
        flashCoroutine = StartCoroutine(DamageFlashCoroutine());
    }
    
    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        Color originalFillColor = fillImage != null ? fillImage.color : Color.white;
        Color originalBackgroundColor = backgroundImage != null ? backgroundImage.color : Color.white;
        
        Color flashColor = Color.white;
        
        if (fillImage != null)
            fillImage.color = flashColor;
        if (backgroundImage != null)
            backgroundImage.color = flashColor;
        
        yield return new WaitForSeconds(flashDuration);
        
        if (fillImage != null)
            fillImage.color = originalFillColor;
        if (backgroundImage != null)
            backgroundImage.color = originalBackgroundColor;
        
        UpdateHealthColor();
        
        flashCoroutine = null;
    }
    
    public float GetHealthPercent()
    {
        return maxHealth > 0 ? currentHealth / maxHealth : 0f;
    }
    
    // Method to manually connect to a specific player (useful for multiplayer or testing)
    public void ConnectToPlayer(Player player)
    {
        // Disconnect from previous player
        if (playerReference != null)
        {
            playerReference.OnHealthChanged -= OnPlayerHealthChanged;
        }
        
        // Connect to new player
        playerReference = player;
        
        if (playerReference != null)
        {
            playerReference.OnHealthChanged += OnPlayerHealthChanged;
            SetMaxHealth(playerReference.MaxHealth);
            SetHealth(playerReference.Health);
        }
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (playerReference != null)
        {
            playerReference.OnHealthChanged -= OnPlayerHealthChanged;
        }
        
        // Stop any running coroutines
        if (healthAnimationCoroutine != null)
        {
            StopCoroutine(healthAnimationCoroutine);
        }
        
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
    }
} 