using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("Ammo Display")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image ammoBar;
    [SerializeField] private string ammoFormat = "{0}"; // current ammo format
    
    [Header("Weapon Info")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image weaponIcon;
    
    [Header("Colors")]
    [SerializeField] private Color normalAmmoColor = Color.white;
    [SerializeField] private Color lowAmmoColor = Color.yellow;
    [SerializeField] private Color emptyAmmoColor = Color.red;
    [SerializeField] private float lowAmmoThreshold = 0.25f; // 25% of max ammo
    
    private PlayerWeaponController weaponController;
    private Weapon currentWeapon;
    
    void Start()
    {
        // Find the player weapon controller
        weaponController = FindObjectOfType<PlayerWeaponController>();
        
        if (weaponController == null)
        {
            Debug.LogWarning("WeaponUI: No PlayerWeaponController found in scene!");
            return;
        }
        
        // Subscribe to weapon events
        weaponController.OnAmmoChanged += UpdateAmmoDisplay;
        
        // Initial setup
        UpdateWeaponInfo();
        UpdateAmmoDisplay(0); // Start with empty display
    }
    
    void Update()
    {
        // Check if weapon changed
        if (weaponController != null && weaponController.CurrentWeapon != currentWeapon)
        {
            currentWeapon = weaponController.CurrentWeapon;
            UpdateWeaponInfo();
            
            if (currentWeapon != null)
            {
                UpdateAmmoDisplay(currentWeapon.CurrentAmmo);
            }
            else
            {
                UpdateAmmoDisplay(0);
            }
        }
    }
    
    private void UpdateAmmoDisplay(int currentAmmo)
    {
        // Update ammo text
        if (ammoText != null)
        {
            if (currentWeapon != null)
            {
                ammoText.text = string.Format(ammoFormat, currentAmmo);
            }
            else
            {
                ammoText.text = "No Weapon";
            }
        }
        
        // Update ammo bar
        if (ammoBar != null && currentWeapon != null)
        {
            float ammoPercentage = (float)currentAmmo / currentWeapon.MaxAmmo;
            ammoBar.fillAmount = ammoPercentage;
            
            // Change color based on ammo level
            Color barColor = normalAmmoColor;
            if (ammoPercentage <= 0f)
            {
                barColor = emptyAmmoColor;
            }
            else if (ammoPercentage <= lowAmmoThreshold)
            {
                barColor = lowAmmoColor;
            }
            
            ammoBar.color = barColor;
        }
        
        // Update text color as well
        if (ammoText != null && currentWeapon != null)
        {
            float ammoPercentage = (float)currentAmmo / currentWeapon.MaxAmmo;
            Color textColor = normalAmmoColor;
            
            if (ammoPercentage <= 0f)
            {
                textColor = emptyAmmoColor;
            }
            else if (ammoPercentage <= lowAmmoThreshold)
            {
                textColor = lowAmmoColor;
            }
            
            ammoText.color = textColor;
        }
    }
    
    private void UpdateWeaponInfo()
    {
        if (currentWeapon != null)
        {
            // Update weapon name
            if (weaponNameText != null)
            {
                weaponNameText.text = currentWeapon.name;
            }
            
            // You could add weapon icon logic here if you have weapon icons
            if (weaponIcon != null)
            {
                weaponIcon.gameObject.SetActive(true);
                // weaponIcon.sprite = currentWeapon.weaponIcon; // If you add weapon icons
            }
        }
        else
        {
            // No weapon equipped
            if (weaponNameText != null)
            {
                weaponNameText.text = "No Weapon";
            }
            
            if (weaponIcon != null)
            {
                weaponIcon.gameObject.SetActive(false);
            }
        }
    }
    
    // Public methods for external use
    public void SetAmmoFormat(string format)
    {
        ammoFormat = format;
        if (currentWeapon != null)
        {
            UpdateAmmoDisplay(currentWeapon.CurrentAmmo);
        }
    }
    
    public void SetLowAmmoThreshold(float threshold)
    {
        lowAmmoThreshold = Mathf.Clamp01(threshold);
        if (currentWeapon != null)
        {
            UpdateAmmoDisplay(currentWeapon.CurrentAmmo);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (weaponController != null)
        {
            weaponController.OnAmmoChanged -= UpdateAmmoDisplay;
        }
    }
} 