using UnityEngine;

public class ItemPickup : MonoBehaviour, ISaveable
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;
    
    [Header("Pickup Settings")]
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool bobUpAndDown = true;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    
    private Vector3 startPosition;
    private AudioSource audioSource;
    private InventoryItem cachedItem;
    
    /// <summary>
    /// Get the current InventoryItem representation of this pickup
    /// </summary>
    public InventoryItem Item 
    { 
        get 
        { 
            if (cachedItem == null && itemData != null)
                cachedItem = itemData.CreateInventoryItem(quantity);
            return cachedItem;
        } 
    }
    
    /// <summary>
    /// Get the ItemData asset this pickup represents
    /// </summary>
    public ItemData ItemData => itemData;
    
    void Start()
    {
        SetupVisuals();
        
        audioSource = GetComponent<AudioSource>();
        
        // Store starting position for bobbing animation
        startPosition = transform.position;
        
        ValidateSetup();
    }
    
    void Update()
    {
        if (bobUpAndDown)
        {
            HandleBobbing();
        }
    }
    
    private void ValidateSetup()
    {
        if (itemData == null && cachedItem == null)
        {
            Debug.LogError($"ItemPickup '{gameObject.name}' has no ItemData assigned and no cached item!", this);
            return;
        }
        
        if (quantity <= 0)
        {
            Debug.LogWarning($"ItemPickup '{gameObject.name}' has invalid quantity ({quantity}). Setting to 1.", this);
            quantity = 1;
        }
    }
    
    private void SetupVisuals()
    {
        // If no sprite renderer assigned, try to get one
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer != null)
        {
            // Try to set sprite from ItemData first
            if (itemData != null && itemData.itemSprite != null)
            {
                spriteRenderer.sprite = itemData.itemSprite;
            }
            // If no ItemData, try to use cached item sprite (for runtime items)
            else if (cachedItem != null && cachedItem.ItemSprite != null)
            {
                spriteRenderer.sprite = cachedItem.ItemSprite;
            }
            // If neither, show warning
            else if (itemData == null && cachedItem == null)
            {
                Debug.LogWarning($"ItemPickup '{gameObject.name}' has no ItemData or cached item for sprite setup.", this);
            }
        }
    }
    
    private void HandleBobbing()
    {
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPosition + Vector3.up * yOffset;
    }
    
    /// <summary>
    /// Called when the item is picked up
    /// </summary>
    public void OnItemPickedUp()
    {
        // Play pickup sound
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        // Spawn pickup effect
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, transform.rotation);
        }
        
        // Destroy or disable the pickup
        if (destroyOnPickup)
        {
            // If we have a sound, wait for it to finish before destroying
            if (pickupSound != null && audioSource != null)
            {
                // Disable visuals but keep audio playing
                if (spriteRenderer != null)
                    spriteRenderer.enabled = false;
                
                Collider2D col = GetComponent<Collider2D>();
                if (col != null)
                    col.enabled = false;
                
                // Destroy after sound duration
                Destroy(gameObject, pickupSound.length);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Set up this pickup with specific ItemData and quantity
    /// </summary>
    public void SetupItem(ItemData newItemData, int newQuantity = 1)
    {
        itemData = newItemData;
        quantity = newQuantity;
        cachedItem = null; // Clear cache
        
        SetupVisuals();
        ValidateSetup();
    }
    
    /// <summary>
    /// Set up this pickup with an existing InventoryItem
    /// </summary>
    public void SetupItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null) 
        {
            Debug.LogError("Cannot setup ItemPickup with null InventoryItem!");
            return;
        }
        
        // Store the inventory item directly and quantity
        quantity = inventoryItem.Quantity;
        cachedItem = new InventoryItem(inventoryItem);
        
        // For runtime-created items (like loot drops), we don't have ItemData
        // So we'll work directly with the cached InventoryItem
        itemData = null; // Explicitly set to null for runtime items
        
        // Set up visuals directly from the InventoryItem
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = inventoryItem.ItemSprite;
        }
        
        Debug.Log($"ItemPickup setup with runtime InventoryItem: {inventoryItem.ItemName} x{inventoryItem.Quantity}");
    }
    
    // For debugging in the editor
    void OnValidate()
    {
        if (itemData != null)
        {
            SetupVisuals();
        }
        
        if (quantity <= 0)
            quantity = 1;
    }
    
    #region ISaveable Implementation
    
    public string GetSaveId()
    {
        Vector2 pos = transform.position;
        int dataId = itemData != null ? itemData.itemId : 0;
        return $"item_{dataId}_{pos.x:F1}_{pos.y:F1}_{GetInstanceID()}";
    }
    
    public object GetSaveData()
    {
        return new ItemPickupSaveDataDetailed
        {
            position = transform.position,
            isPickedUp = !gameObject.activeInHierarchy || !gameObject.activeSelf,
            itemData = SerializableInventoryItem.FromInventoryItem(Item),
            destroyOnPickup = destroyOnPickup,
            bobUpAndDown = bobUpAndDown,
            bobSpeed = bobSpeed,
            bobHeight = bobHeight,
            startPosition = startPosition
        };
    }
    
    public void LoadSaveData(object data)
    {
        if (data is ItemPickupSaveDataDetailed saveData)
        {
            // Restore position
            transform.position = saveData.position;
            startPosition = saveData.startPosition;
            
            // Restore item data
            if (saveData.itemData != null)
            {
                quantity = saveData.itemData.quantity;
                cachedItem = saveData.itemData.ToInventoryItem();
                
                SetupVisuals();
            }
            
            // Restore pickup state
            if (saveData.isPickedUp)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                
                // Restore settings
                destroyOnPickup = saveData.destroyOnPickup;
                bobUpAndDown = saveData.bobUpAndDown;
                bobSpeed = saveData.bobSpeed;
                bobHeight = saveData.bobHeight;
            }
            
            Debug.Log($"ItemPickup data loaded: Item={itemData?.itemName}, Quantity={quantity}, Position={transform.position}, PickedUp={saveData.isPickedUp}");
        }
    }
    
    #endregion
}

/// <summary>
/// Detailed save data for ItemPickup
/// </summary>
[System.Serializable]
public class ItemPickupSaveDataDetailed
{
    public Vector3 position;
    public bool isPickedUp;
    public SerializableInventoryItem itemData;
    public bool destroyOnPickup;
    public bool bobUpAndDown;
    public float bobSpeed;
    public float bobHeight;
    public Vector3 startPosition;
} 