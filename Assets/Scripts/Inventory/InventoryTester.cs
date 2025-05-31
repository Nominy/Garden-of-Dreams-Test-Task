using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private Sprite[] testItemSprites;
    [SerializeField] private string[] testItemNames = { "Apple", "Sword", "Potion", "Key", "Shield" };
    
    [Header("Test Controls")]
    [SerializeField] private KeyCode addItemKey = KeyCode.Space;
    [SerializeField] private KeyCode removeAllItemsKey = KeyCode.R;
    
    private int nextItemId = 1;
    
    void Start()
    {
        // Find inventory system if not assigned
        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }
        
        if (inventorySystem == null)
        {
            Debug.LogError("No InventorySystem found! Please assign one or add it to the scene.");
            return;
        }
        
        // Subscribe to inventory events for debugging
        inventorySystem.OnItemAdded += OnItemAdded;
        inventorySystem.OnItemRemoved += OnItemRemoved;
        inventorySystem.OnInventoryChanged += OnInventoryChanged;
        
        Debug.Log($"Inventory Tester initialized. Press {addItemKey} to add random items, {removeAllItemsKey} to clear inventory.");
    }
    
    void Update()
    {
        if (inventorySystem == null) return;
        
        if (Input.GetKeyDown(addItemKey))
        {
            AddRandomItem();
        }
        
        if (Input.GetKeyDown(removeAllItemsKey))
        {
            ClearInventory();
        }
    }
    
    /// <summary>
    /// Add a random test item to the inventory
    /// </summary>
    public void AddRandomItem()
    {
        if (testItemSprites == null || testItemSprites.Length == 0)
        {
            Debug.LogWarning("No test item sprites assigned!");
            return;
        }
        
        // Pick random sprite and name
        int randomIndex = Random.Range(0, testItemSprites.Length);
        Sprite sprite = testItemSprites[randomIndex];
        
        string itemName = "Item";
        if (testItemNames != null && testItemNames.Length > 0)
        {
            int nameIndex = Random.Range(0, testItemNames.Length);
            itemName = testItemNames[nameIndex];
        }
        
        // Create and add item with random quantity
        int randomQuantity = Random.Range(1, 6); // 1-5 items
        InventoryItem newItem = new InventoryItem(itemName, sprite, nextItemId++, randomQuantity);
        bool added = inventorySystem.AddItem(newItem);
        
        if (!added)
        {
            Debug.Log("Failed to add item - inventory might be full!");
        }
    }
    
    /// <summary>
    /// Add a specific item to the inventory (useful for external scripts)
    /// </summary>
    /// <param name="itemName">Name of the item</param>
    /// <param name="sprite">Sprite for the item</param>
    /// <returns>True if item was added successfully</returns>
    public bool AddSpecificItem(string itemName, Sprite sprite)
    {
        if (inventorySystem == null || sprite == null)
            return false;
        
        InventoryItem newItem = new InventoryItem(itemName, sprite, nextItemId++, 1);
        return inventorySystem.AddItem(newItem);
    }
    
    /// <summary>
    /// Clear all items from inventory
    /// </summary>
    public void ClearInventory()
    {
        if (inventorySystem == null) return;
        
        var allItems = inventorySystem.GetAllItems();
        for (int i = 0; i < allItems.Count; i++)
        {
            if (allItems[i] != null)
            {
                inventorySystem.RemoveItemAt(i);
            }
        }
        
        Debug.Log("Inventory cleared!");
    }
    
    // Event handlers for debugging
    private void OnItemAdded(InventoryItem item)
    {
        Debug.Log($"Item added: {item.ItemName} x{item.Quantity} (ID: {item.ItemId})");
    }
    
    private void OnItemRemoved(InventoryItem item)
    {
        Debug.Log($"Item removed: {item.ItemName} x{item.Quantity} (ID: {item.ItemId})");
    }
    
    private void OnInventoryChanged()
    {
        Debug.Log($"Inventory changed. Used slots: {inventorySystem.UsedSlots}/{inventorySystem.MaxSlots}");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to avoid memory leaks
        if (inventorySystem != null)
        {
            inventorySystem.OnItemAdded -= OnItemAdded;
            inventorySystem.OnItemRemoved -= OnItemRemoved;
            inventorySystem.OnInventoryChanged -= OnInventoryChanged;
        }
    }
} 