using UnityEngine;

[System.Serializable]
public class WeaponStats
{
    [Header("Damage")]
    public float damage = 10f;
    
    [Header("Firing")]
    public float fireRate = 1f; // Shots per second
    public float bulletSpeed = 15f;
    public float bulletLifetime = 3f;
    
    [Header("Ammo")]
    public int maxAmmo = 30;
    public int maxReserveAmmo = 120;
    public float reloadTime = 2f;
    
    [Header("Accuracy")]
    public float spread = 0f; // Degrees of spread
    public float recoil = 0f; // Amount of recoil
}

public class Weapon : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private WeaponStats stats;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    
    [Header("Effects")]
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;
    
    // Ammo system
    private int currentAmmo;
    private int reserveAmmo;
    private bool isReloading = false;
    
    // Firing system
    private float lastFireTime = 0f;
    private AudioSource audioSource;
    
    // Events
    public System.Action<int, int> OnAmmoChanged; // currentAmmo, reserveAmmo
    public System.Action<float> OnReloadStarted; // reload duration
    public System.Action OnReloadFinished;
    public System.Action OnWeaponFired;
    public System.Action OnWeaponEmpty;
    
    // Properties
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public int MaxAmmo => stats.maxAmmo;
    public bool IsReloading => isReloading;
    public bool CanFire => !isReloading && currentAmmo > 0 && Time.time >= lastFireTime + (1f / stats.fireRate);
    public bool CanReload => !isReloading && currentAmmo < stats.maxAmmo && reserveAmmo > 0;
    public WeaponStats Stats => stats;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize ammo
        currentAmmo = stats.maxAmmo;
        reserveAmmo = stats.maxReserveAmmo;
        
        // Validate setup
        if (bulletPrefab == null)
        {
            Debug.LogError($"Bullet prefab not assigned to weapon {gameObject.name}!");
        }
        
        if (firePoint == null)
        {
            Debug.LogWarning($"Fire point not assigned to weapon {gameObject.name}. Using weapon position.");
            firePoint = transform;
        }
        
        // Notify UI of initial ammo state
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }
    
    public bool TryFire(Vector2 direction)
    {
        if (!CanFire)
        {
            // Play empty sound if out of ammo
            if (currentAmmo <= 0 && emptySound != null)
            {
                audioSource.PlayOneShot(emptySound);
                OnWeaponEmpty?.Invoke();
            }
            return false;
        }
        
        Fire(direction);
        return true;
    }
    
    private void Fire(Vector2 direction)
    {
        // Update fire timing
        lastFireTime = Time.time;
        
        // Consume ammo
        currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
        
        // Apply spread
        Vector2 fireDirection = ApplySpread(direction);
        
        // Create bullet
        if (bulletPrefab != null)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            
            if (bullet != null)
            {
                bullet.Initialize(fireDirection, stats.bulletSpeed, stats.damage, true);
            }
            else
            {
                Debug.LogError($"Bullet prefab {bulletPrefab.name} doesn't have a Bullet component!");
                Destroy(bulletObj);
            }
        }
        
        // Effects
        PlayFireEffects();
        
        // Trigger events
        OnWeaponFired?.Invoke();
    }
    
    public bool TryReload()
    {
        if (!CanReload) return false;
        
        StartCoroutine(ReloadCoroutine());
        return true;
    }
    
    private System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        OnReloadStarted?.Invoke(stats.reloadTime);
        
        // Play reload sound
        if (reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
        
        yield return new WaitForSeconds(stats.reloadTime);
        
        // Calculate how much ammo to reload
        int ammoNeeded = stats.maxAmmo - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);
        
        // Update ammo counts
        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;
        
        isReloading = false;
        
        // Trigger events
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
        OnReloadFinished?.Invoke();
    }
    
    private Vector2 ApplySpread(Vector2 baseDirection)
    {
        if (stats.spread <= 0f) return baseDirection;
        
        // Calculate random spread angle
        float spreadAngle = Random.Range(-stats.spread / 2f, stats.spread / 2f);
        
        // Convert direction to angle, apply spread, convert back
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        float finalAngle = (baseAngle + spreadAngle) * Mathf.Deg2Rad;
        
        return new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));
    }
    
    private void PlayFireEffects()
    {
        // Play fire sound
        if (fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        // Show muzzle flash
        if (muzzleFlash != null)
        {
            StartCoroutine(ShowMuzzleFlash());
        }
    }
    
    private System.Collections.IEnumerator ShowMuzzleFlash()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        muzzleFlash.SetActive(false);
    }
    
    // Utility methods
    public void AddAmmo(int amount)
    {
        reserveAmmo = Mathf.Min(reserveAmmo + amount, stats.maxReserveAmmo);
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }
    
    public void SetAmmo(int current, int reserve)
    {
        currentAmmo = Mathf.Clamp(current, 0, stats.maxAmmo);
        reserveAmmo = Mathf.Clamp(reserve, 0, stats.maxReserveAmmo);
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }
    
    // Auto-reload when empty (optional)
    public void EnableAutoReload()
    {
        OnWeaponEmpty += () => {
            if (CanReload)
            {
                TryReload();
            }
        };
    }
} 