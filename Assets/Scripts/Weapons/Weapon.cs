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
    
    [Header("Accuracy")]
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
    [SerializeField] private AudioClip emptySound;
    
    // Ammo system
    private int currentAmmo;
    
    // Firing system
    private float lastFireTime = 0f;
    private AudioSource audioSource;
    
    // Events
    public System.Action<int> OnAmmoChanged; // currentAmmo only
    public System.Action OnWeaponFired;
    public System.Action OnWeaponEmpty;
    
    // Properties
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => stats.maxAmmo;
    public bool CanFire => currentAmmo > 0 && Time.time >= lastFireTime + (1f / stats.fireRate);
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
        OnAmmoChanged?.Invoke(currentAmmo);
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
        OnAmmoChanged?.Invoke(currentAmmo);
        
        // Create bullet
        if (bulletPrefab != null)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            
            if (bullet != null)
            {
                bullet.Initialize(direction, stats.bulletSpeed, stats.damage, true);
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
        currentAmmo = Mathf.Min(currentAmmo + amount, stats.maxAmmo);
        OnAmmoChanged?.Invoke(currentAmmo);
    }
    
    public void SetAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(amount, 0, stats.maxAmmo);
        OnAmmoChanged?.Invoke(currentAmmo);
    }
    
    public void RefillAmmo()
    {
        currentAmmo = stats.maxAmmo;
        OnAmmoChanged?.Invoke(currentAmmo);
    }
} 