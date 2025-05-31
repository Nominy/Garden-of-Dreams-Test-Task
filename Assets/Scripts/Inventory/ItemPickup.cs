using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    private InventoryItem item;
    [SerializeField] private string itemName = "Default Item";
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private int itemId = 1;
    [SerializeField] private int quantity = 1;
    [SerializeField] private int maxStackSize = 99;
    
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
    
    public InventoryItem Item => item;
    
    void Start()
    {
        CreateItem();
        
        SetupVisuals();
        
        audioSource = GetComponent<AudioSource>();
        
        // Store starting position for bobbing animation
        startPosition = transform.position;
    }
    
    void Update()
    {
        if (bobUpAndDown)
        {
            HandleBobbing();
        }
    }
    
    private void CreateItem()
    {
        item = new InventoryItem(itemName, itemSprite, itemId, quantity, maxStackSize);
    }
    
    private void SetupVisuals()
    {
        // If no sprite renderer assigned, try to get one
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Set sprite if available
        if (spriteRenderer != null && item != null && item.ItemSprite != null)
        {
            spriteRenderer.sprite = item.ItemSprite;
        }
        else if (spriteRenderer != null && itemSprite != null)
        {
            spriteRenderer.sprite = itemSprite;
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
    /// Set up this pickup with specific item data
    /// </summary>
    /// <param name="itemName">Name of the item</param>
    /// <param name="sprite">Sprite for the item</param>
    /// <param name="id">Unique ID for the item</param>
    public void SetupItem(string itemName, Sprite sprite, int id)
    {
        SetupItem(itemName, sprite, id, 1, 99);
    }
    
    /// <summary>
    /// Set up this pickup with specific item data including quantity
    /// </summary>
    /// <param name="itemName">Name of the item</param>
    /// <param name="sprite">Sprite for the item</param>
    /// <param name="id">Unique ID for the item</param>
    /// <param name="quantity">Quantity of the item</param>
    /// <param name="maxStackSize">Maximum stack size for the item</param>
    public void SetupItem(string itemName, Sprite sprite, int id, int quantity, int maxStackSize)
    {
        this.itemName = itemName;
        this.itemSprite = sprite;
        this.itemId = id;
        this.quantity = quantity;
        this.maxStackSize = maxStackSize;
        
        CreateItem();
        SetupVisuals();
    }
    
    /// <summary>
    /// Set up this pickup with an existing InventoryItem
    /// </summary>
    /// <param name="inventoryItem">The item to represent</param>
    public void SetupItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null) return;
        
        item = new InventoryItem(inventoryItem);
        itemName = inventoryItem.ItemName;
        itemSprite = inventoryItem.ItemSprite;
        itemId = inventoryItem.ItemId;
        quantity = inventoryItem.Quantity;
        maxStackSize = inventoryItem.MaxStackSize;
        
        SetupVisuals();
    }
    
    // For debugging in the editor
    void OnValidate()
    {
        if (item == null && !string.IsNullOrEmpty(itemName) && itemSprite != null)
        {
            CreateItem();
        }
    }
} 