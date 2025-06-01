using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject healthBarContainer;
    
    [Header("Colors")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    
    [Header("Settings")]
    [SerializeField] private float hideDelay = 3f; // Time to hide health bar after taking damage
    [SerializeField] private bool alwaysShowWhenDamaged = true;
    
    private float maxHealth;
    private float currentHealth;
    private bool isVisible = false;
    private Camera playerCamera;
    private Canvas canvas;
    
    void Awake()
    {
        // Find player camera
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();
        
        // Setup canvas if not already present
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = playerCamera;
        }
        
        // Setup health slider if not assigned
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();
        
        // Setup fill image if not assigned
        if (fillImage == null && healthSlider != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        
        // Start hidden
        SetVisible(false);
    }
    
    void Start()
    {
        // Ensure the health bar faces the camera
        if (playerCamera != null)
        {
            transform.LookAt(transform.position + playerCamera.transform.forward);
        }
    }
    
    void LateUpdate()
    {
        // Always face the camera
        if (playerCamera != null && isVisible)
        {
            transform.LookAt(transform.position + playerCamera.transform.forward);
        }
    }
    
    public void SetMaxHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
        
        UpdateHealthColor();
    }
    
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        
        UpdateHealthColor();
        
        // Show health bar when taking damage
        if (currentHealth < maxHealth && alwaysShowWhenDamaged)
        {
            SetVisible(true);
            
            // Hide after delay if health is full
            if (currentHealth >= maxHealth)
            {
                Invoke(nameof(HideHealthBar), hideDelay);
            }
        }
    }
    
    public void SetVisible(bool visible)
    {
        isVisible = visible;
        
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(visible);
        }
        else
        {
            gameObject.SetActive(visible);
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
    
    private void HideHealthBar()
    {
        if (currentHealth >= maxHealth)
        {
            SetVisible(false);
        }
    }
    
    public float GetHealthPercent()
    {
        return maxHealth > 0 ? currentHealth / maxHealth : 0f;
    }
    
    public bool IsVisible => isVisible;
} 