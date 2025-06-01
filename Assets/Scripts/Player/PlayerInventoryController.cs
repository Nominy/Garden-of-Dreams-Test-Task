using UnityEngine;

/// <summary>
/// Handles player inventory management and item pickup.
/// Refactored to work with the new Player architecture.
/// </summary>
public class PlayerInventoryController : PlayerControllerBase
{
    [Header("Inventory References")]
    [SerializeField] private InventorySystem inventorySystem;
    
    [Header("Input Settings")]
    [SerializeField] private GameObject inventoryUI;
    
    protected override void OnInitialize()
    {
        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }
        
        if (inventorySystem == null)
        {
            Debug.LogWarning("PlayerInventoryController: No InventorySystem found in scene!");
        }
    }
        
    public bool AddItem(InventoryItem item)
    {
        if (!IsReady || inventorySystem == null)
        {
            Debug.LogWarning("PlayerInventoryController: Cannot add item - controller not ready or no inventory system!");
            return false;
        }
        
        return inventorySystem.AddItem(item);
    }
    
    public bool AddItem(InventoryItem item, int quantity)
    {
        if (!IsReady || inventorySystem == null)
        {
            Debug.LogWarning("PlayerInventoryController: Cannot add item - controller not ready or no inventory system!");
            return false;
        }
        
        return inventorySystem.AddItem(item, quantity);
    }
    
    public bool AddItem(string itemName, Sprite sprite, int itemId = -1, int quantity = 1, int maxStackSize = 99)
    {
        if (inventorySystem == null || sprite == null)
            return false;
        
        if (itemId == -1)
        {
            itemId = Random.Range(1000, 9999);
        }
        
        InventoryItem newItem = new InventoryItem(itemName, sprite, itemId, quantity, maxStackSize);
        return inventorySystem.AddItem(newItem);
    }
    
    public bool HasInventorySpace()
    {
        return IsReady && inventorySystem != null && inventorySystem.HasSpace();
    }
    
    public InventorySystem GetInventorySystem()
    {
        return inventorySystem;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsReady) return;
        
        if (enableDebugLogs)
            Debug.Log("PlayerInventoryController: OnTriggerEnter2D");
            
        ItemPickup itemPickup = other.GetComponent<ItemPickup>();
        if (itemPickup != null)
        {
            if (AddItem(itemPickup.Item))
            {
                Debug.Log($"PlayerInventoryController: Picked up {itemPickup.Item.ItemName}");
                itemPickup.OnItemPickedUp();
            }
            else
            {
                Debug.Log("PlayerInventoryController: Inventory is full!");
            }
        }
    }
} 