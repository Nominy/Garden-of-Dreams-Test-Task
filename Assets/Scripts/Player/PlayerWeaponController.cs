using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Weapon System")]
    [SerializeField] private Weapon currentWeapon;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform aimPivot; // Point that rotates to aim
    
    [Header("Controls")]
    [SerializeField] private Button fireButton;
    [SerializeField] private Button reloadButton;
    [SerializeField] private bool useMouseAiming = true;
    [SerializeField] private bool aimWithMovement = false; // Aim in movement direction when not using mouse
    
    [Header("Aiming")]
    [SerializeField] private bool flipWeaponWithDirection = true;
    
    private PlayerController playerController;
    private Camera mainCamera;
    private Vector2 lastAimDirection = Vector2.right; // Default facing right
    
    // Events for UI/audio feedback
    public System.Action<int, int> OnAmmoChanged;
    public System.Action<float> OnReloadStarted;
    public System.Action OnReloadFinished;
    public System.Action OnWeaponFired;
    public System.Action OnWeaponEmpty;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // Set up weapon events if weapon exists
        if (currentWeapon != null)
        {
            SetupWeaponEvents();
        }
        
        // Validate setup
        if (fireButton == null)
        {
            Debug.LogWarning("Fire button not assigned to PlayerWeaponController!");
        }
        
        if (aimPivot == null)
        {
            Debug.LogWarning("Aim pivot not assigned. Using weapon holder transform.");
            aimPivot = weaponHolder != null ? weaponHolder : transform;
        }
    }
    
    void Update()
    {
        HandleAiming();
        HandleFireInput();
        HandleReloadInput();
    }
    
    private void HandleAiming()
    {
        Vector2 aimDirection = GetAimDirection();
        
        if (aimDirection.magnitude > 0.1f)
        {
            lastAimDirection = aimDirection.normalized;
            
            // Rotate aim pivot to face the aim direction
            if (aimPivot != null)
            {
                float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                aimPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                
                // Flip weapon sprite if needed
                if (flipWeaponWithDirection && currentWeapon != null)
                {
                    bool shouldFlip = aimDirection.x < 0;
                    Vector3 weaponScale = currentWeapon.transform.localScale;
                    weaponScale.y = shouldFlip ? -Mathf.Abs(weaponScale.y) : Mathf.Abs(weaponScale.y);
                    currentWeapon.transform.localScale = weaponScale;
                }
            }
        }
    }
    
    private Vector2 GetAimDirection()
    {
        if (useMouseAiming && mainCamera != null)
        {
            // Mouse aiming
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));
            return ((Vector2)worldMousePos - (Vector2)transform.position).normalized;
        }
        else if (aimWithMovement && playerController != null)
        {
            // Aim in movement direction
            Vector2 moveDirection = Vector2.zero;
            
            // Try to get movement direction from player controller
            if (playerController.IsMovingHorizontally)
            {
                moveDirection.x = playerController.FacingRight ? 1f : -1f;
            }
            
            if (moveDirection.magnitude > 0.1f)
            {
                return moveDirection.normalized;
            }
        }
        
        // Default: maintain last aim direction or face player's facing direction
        if (playerController != null)
        {
            return playerController.FacingRight ? Vector2.right : Vector2.left;
        }
        
        return lastAimDirection;
    }
    
    private void HandleFireInput()
    {
        if (fireButton == null || currentWeapon == null) return;
        
        if (fireButton.IsPressed)
        {
            TryFire();
        }
    }
    
    private void HandleReloadInput()
    {
        if (reloadButton == null || currentWeapon == null) return;
        
        if (reloadButton.WasClicked)
        {
            TryReload();
        }
    }
    
    public bool TryFire()
    {
        if (currentWeapon == null) return false;
        
        Vector2 fireDirection = lastAimDirection;
        bool fired = currentWeapon.TryFire(fireDirection);
        
        if (fired)
        {
            // Trigger attack animation
            if (playerController != null)
            {
                playerController.TriggerAttack();
            }
        }
        
        return fired;
    }
    
    public bool TryReload()
    {
        if (currentWeapon == null) return false;
        
        return currentWeapon.TryReload();
    }
    
    public void EquipWeapon(Weapon newWeapon)
    {
        // Unequip current weapon
        if (currentWeapon != null)
        {
            RemoveWeaponEvents();
            
            if (weaponHolder != null)
            {
                currentWeapon.transform.SetParent(null);
            }
        }
        
        // Equip new weapon
        currentWeapon = newWeapon;
        
        if (currentWeapon != null)
        {
            if (weaponHolder != null)
            {
                currentWeapon.transform.SetParent(weaponHolder);
                currentWeapon.transform.localPosition = Vector3.zero;
                currentWeapon.transform.localRotation = Quaternion.identity;
            }
            
            SetupWeaponEvents();
        }
    }
    
    private void SetupWeaponEvents()
    {
        if (currentWeapon == null) return;
        
        currentWeapon.OnAmmoChanged += (current, reserve) => OnAmmoChanged?.Invoke(current, reserve);
        currentWeapon.OnReloadStarted += (duration) => OnReloadStarted?.Invoke(duration);
        currentWeapon.OnReloadFinished += () => OnReloadFinished?.Invoke();
        currentWeapon.OnWeaponFired += () => OnWeaponFired?.Invoke();
        currentWeapon.OnWeaponEmpty += () => OnWeaponEmpty?.Invoke();
    }
    
    private void RemoveWeaponEvents()
    {
        if (currentWeapon == null) return;
        
        currentWeapon.OnAmmoChanged -= (current, reserve) => OnAmmoChanged?.Invoke(current, reserve);
        currentWeapon.OnReloadStarted -= (duration) => OnReloadStarted?.Invoke(duration);
        currentWeapon.OnReloadFinished -= () => OnReloadFinished?.Invoke();
        currentWeapon.OnWeaponFired -= () => OnWeaponFired?.Invoke();
        currentWeapon.OnWeaponEmpty -= () => OnWeaponEmpty?.Invoke();
    }
    
    // Public properties for UI/other systems
    public Weapon CurrentWeapon => currentWeapon;
    public bool HasWeapon => currentWeapon != null;
    public Vector2 AimDirection => lastAimDirection;
    
    // Utility methods
    public void SetAimMode(bool useMouseAiming, bool aimWithMovement = false)
    {
        this.useMouseAiming = useMouseAiming;
        this.aimWithMovement = aimWithMovement;
    }
    
    void OnDestroy()
    {
        RemoveWeaponEvents();
    }
} 