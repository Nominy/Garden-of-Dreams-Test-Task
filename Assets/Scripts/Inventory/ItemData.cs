using UnityEngine;

/// <summary>
/// ScriptableObject that defines all properties of an item.
/// This serves as the single source of truth for item data.
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName = "New Item";
    public int itemId = 1;
    
    [Header("Visual")]
    public Sprite itemSprite;
    
    [Header("Stacking")]
    public int maxStackSize = 99;
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description = "";
    
    /// <summary>
    /// Create an InventoryItem from this ItemData with specified quantity
    /// </summary>
    public InventoryItem CreateInventoryItem(int quantity = 1)
    {
        return new InventoryItem(itemName, itemSprite, itemId, quantity, maxStackSize);
    }
    
    /// <summary>
    /// Validate item data in the editor
    /// </summary>
    private void OnValidate()
    {
        // Ensure item ID is positive
        if (itemId <= 0)
            itemId = 1;
            
        // Ensure max stack size is at least 1
        if (maxStackSize <= 0)
            maxStackSize = 1;
            
        // Ensure item name is not empty
        if (string.IsNullOrEmpty(itemName))
            itemName = "New Item";
    }
} 