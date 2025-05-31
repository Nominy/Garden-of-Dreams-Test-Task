using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private int itemId;
    [SerializeField] private int quantity = 1;
    [SerializeField] private int maxStackSize = 99;
    
    public string ItemName => itemName;
    public Sprite ItemSprite => itemSprite;
    public int ItemId => itemId;
    public int Quantity => quantity;
    public int MaxStackSize => maxStackSize;
    
    public InventoryItem(string name, Sprite sprite, int id, int qty = 1, int maxStack = 99)
    {
        itemName = name;
        itemSprite = sprite;
        itemId = id;
        quantity = Mathf.Max(1, qty);
        maxStackSize = Mathf.Max(1, maxStack);
    }
    
    public InventoryItem(InventoryItem other)
    {
        itemName = other.itemName;
        itemSprite = other.itemSprite;
        itemId = other.itemId;
        quantity = other.quantity;
        maxStackSize = other.maxStackSize;
    }
    
    /// <summary>
    /// Check if this item can stack with another item
    /// </summary>
    public bool CanStackWith(InventoryItem other)
    {
        if (other == null) return false;
        return itemId == other.itemId && itemName == other.itemName;
    }
    
    /// <summary>
    /// Check if this stack has room for more items
    /// </summary>
    public bool HasRoom => quantity < maxStackSize;
    
    /// <summary>
    /// Get how many more items can be added to this stack
    /// </summary>
    public int RemainingSpace => maxStackSize - quantity;
    
    /// <summary>
    /// Add quantity to this item stack
    /// </summary>
    /// <param name="amount">Amount to add</param>
    /// <returns>Amount that couldn't be added (overflow)</returns>
    public int AddQuantity(int amount)
    {
        int canAdd = Mathf.Min(amount, RemainingSpace);
        quantity += canAdd;
        return amount - canAdd;
    }
    
    /// <summary>
    /// Remove quantity from this item stack
    /// </summary>
    /// <param name="amount">Amount to remove</param>
    /// <returns>Amount actually removed</returns>
    public int RemoveQuantity(int amount)
    {
        int canRemove = Mathf.Min(amount, quantity);
        quantity -= canRemove;
        return canRemove;
    }
    
    /// <summary>
    /// Check if this stack is empty
    /// </summary>
    public bool IsEmpty => quantity <= 0;
} 