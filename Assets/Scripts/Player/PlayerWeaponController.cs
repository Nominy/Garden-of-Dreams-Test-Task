using UnityEngine;

/// <summary>
/// Handles player weapon management, aiming, and firing.
/// Refactored to work with the new Player architecture.
/// </summary>
public class PlayerWeaponController : PlayerControllerBase
{
    [Header("Weapon System")]
    [SerializeField] private Weapon currentWeapon;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform aimPivot; // Point that rotates to aim
    
    [Header("Controls")]
    [SerializeField] private HoldableButton fireButton;
    [SerializeField] private bool useMouseAiming = true;
    [SerializeField] private bool aimWithMovement = false; // Aim in movement direction when not using mouse
    
    [Header("Aiming")]
    [SerializeField] private bool flipWeaponWithDirection = true;
    
    private Camera mainCamera;
    private Vector2 lastAimDirection = Vector2.right; // Default facing right
    
    // Events for UI/audio feedback
    public System.Action<int> OnAmmoChanged; // Only current ammo
    public System.Action OnWeaponFired;
    public System.Action OnWeaponEmpty;
    
    protected override void OnInitialize()
    {
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
            Debug.LogWarning("PlayerWeaponController: Fire button not assigned!");
        }
        
        if (aimPivot == null)
        {
            Debug.LogWarning("PlayerWeaponController: Aim pivot not assigned. Using weapon holder transform.");
            aimPivot = weaponHolder != null ? weaponHolder : transform;
        }
    }
    
    void Update()
    {
        if (!IsReady) return;
        
        HandleAiming();
        HandleFireInput();
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
        else if (aimWithMovement && player != null && player.Movement != null)
        {
            // Aim in movement direction
            Vector2 moveDirection = Vector2.zero;
            
            // Try to get movement direction from player movement controller
            if (player.Movement.IsMovingHorizontally)
            {
                moveDirection.x = player.Movement.FacingRight ? 1f : -1f;
            }
            
            if (moveDirection.magnitude > 0.1f)
            {
                return moveDirection.normalized;
            }
        }
        
        // Default: maintain last aim direction or face player's facing direction
        if (player != null && player.Movement != null)
        {
            return player.Movement.FacingRight ? Vector2.right : Vector2.left;
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
    
    public bool TryFire()
    {
        if (currentWeapon == null) return false;
        
        Vector2 fireDirection = lastAimDirection;
        bool fired = currentWeapon.TryFire(fireDirection);
        
        if (fired)
        {
            // Trigger attack animation through Player
            if (player != null)
            {
                player.TriggerAttack();
            }
        }
        
        return fired;
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
        
        currentWeapon.OnAmmoChanged += (current) => OnAmmoChanged?.Invoke(current);
        currentWeapon.OnWeaponFired += () => OnWeaponFired?.Invoke();
        currentWeapon.OnWeaponEmpty += () => OnWeaponEmpty?.Invoke();
    }
    
    private void RemoveWeaponEvents()
    {
        if (currentWeapon == null) return;
        
        currentWeapon.OnAmmoChanged -= (current) => OnAmmoChanged?.Invoke(current);
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
    
    protected override void OnCleanup()
    {
        RemoveWeaponEvents();
    }
} 