using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    [Header("Inventory References")]
    [SerializeField] private InventorySystem inventorySystem;
    
    [Header("Input Settings")]
    [SerializeField] private GameObject inventoryUI;
    
    void Start()
    {
        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }
    }
        
    public bool AddItem(InventoryItem item)
    {
        if (inventorySystem == null)
        {
            Debug.LogWarning("No inventory system assigned to PlayerInventoryController!");
            return false;
        }
        
        return inventorySystem.AddItem(item);
    }
    
    public bool AddItem(InventoryItem item, int quantity)
    {
        if (inventorySystem == null)
        {
            Debug.LogWarning("No inventory system assigned to PlayerInventoryController!");
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
        return inventorySystem != null && inventorySystem.HasSpace();
    }
    
    public InventorySystem GetInventorySystem()
    {
        return inventorySystem;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D");
        ItemPickup itemPickup = other.GetComponent<ItemPickup>();
        if (itemPickup != null)
        {
            if (AddItem(itemPickup.Item))
            {
                Debug.Log($"Picked up: {itemPickup.Item.ItemName}");
                itemPickup.OnItemPickedUp();
            }
            else
            {
                Debug.Log("Inventory is full!");
            }
        }
    }
} 