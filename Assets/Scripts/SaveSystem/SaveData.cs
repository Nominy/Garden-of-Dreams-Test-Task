using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main container for all save data.
/// Uses strongly-typed collections for robust JSON serialization.
/// </summary>
[System.Serializable]
public class SaveData
{
    public string saveVersion = "1.0";
    public DateTime saveTime;
    public float timePlayed;
    public string currentScene;

    // Strongly-typed collections for saveable entities
    public PlayerSaveDataDetailed playerDetailedData; // Assuming single player
    public List<BasicEnemySaveData> enemySaveDataList = new List<BasicEnemySaveData>();
    public List<ItemPickupSaveDataDetailed> itemPickupSaveDataList = new List<ItemPickupSaveDataDetailed>();
    // Add other lists here for other ISaveable types if any, e.g.:
    // public List<AnotherCustomSaveData> anotherCustomSaveDataList = new List<AnotherCustomSaveData>();

    public SaveData()
    {
        saveTime = DateTime.Now;
        timePlayed = 0f;
        currentScene = "";
    }
}

// Legacy data structures below are kept if GetSaveFileInfo or other minor non-core gameplay save logic still uses them.
// Otherwise, they can be removed or fully migrated to the new detailed structures.
// For now, ensuring they don't interfere with the main save/load.

/// <summary>
/// Player-specific save data (Legacy, primarily for SaveFileInfo)
/// </summary>
[System.Serializable]
public class PlayerSaveData // This is the SIMPLER one, used by GetSaveFileInfo
{
    public Vector2 position;
    public float currentHealth;
    public float maxHealth;
    public bool isAlive;
    
    public PlayerSaveData()
    {
        position = Vector2.zero;
        currentHealth = 100f;
        maxHealth = 100f;
        isAlive = true;
    }
    
    public PlayerSaveData(Vector2 pos, float health, float maxHp, bool alive)
    {
        position = pos;
        currentHealth = health;
        maxHealth = maxHp;
        isAlive = alive;
    }
}

// The following EnemySaveData and ItemPickupSaveData (simpler versions)
// are now effectively replaced by BasicEnemySaveData and ItemPickupSaveDataDetailed
// for actual game state saving. They can be removed if not used elsewhere (e.g. GetSaveFileInfo)

/// <summary>
/// Enemy-specific save data (Legacy, replaced by BasicEnemySaveData within ISaveable context)
/// </summary>
[System.Serializable]
public class EnemySaveData
{
    public string enemyId;
    public string enemyType;
    public Vector2 position;
    public float currentHealth;
    public float maxHealth;
    public bool isAlive;
    public bool isActive;
    
    public EnemySaveData() { /* ... */ }
    public EnemySaveData(string id, string type, Vector2 pos, float health, float maxHp, bool alive, bool active) { /* ... */ }
}

/// <summary>
/// World item pickup save data (Legacy, replaced by ItemPickupSaveDataDetailed)
/// </summary>
[System.Serializable]
public class ItemPickupSaveData 
{
    public string itemId;
    public Vector2 position;
    public SerializableInventoryItem itemData;
    public bool isPickedUp;
    
    public ItemPickupSaveData() { /* ... */ }
    public ItemPickupSaveData(string id, Vector2 pos, SerializableInventoryItem item, bool pickedUp) { /* ... */ }
}

/// <summary>
/// Inventory save data (Legacy, player inventory now saved within PlayerSaveDataDetailed)
/// </summary>
[System.Serializable]
public class InventorySaveData
{
    public List<SerializableInventoryItem> items = new List<SerializableInventoryItem>();
    public int maxSlots;
    
    public InventorySaveData() { /* ... */ }
    public InventorySaveData(List<SerializableInventoryItem> inventoryItems, int slots) { /* ... */ }
}

/// <summary>
/// JSON-serializable version of InventoryItem.
/// Improved to reference ItemData by ID for more reliable serialization.
/// </summary>
[System.Serializable]
public class SerializableInventoryItem
{
    // New approach: Reference ItemData by ID (primary)
    public int itemDataId;
    public int quantity;
    
    // Legacy fields: Keep for backward compatibility with old saves
    public string itemName;
    public int itemId;
    public int maxStackSize;
    public string spritePath;
    
    public SerializableInventoryItem() 
    {
        itemDataId = 0;
        quantity = 1;
        itemName = "";
        itemId = 0;
        maxStackSize = 99;
        spritePath = "";
    }
    
    public SerializableInventoryItem(int dataId, int qty)
    {
        itemDataId = dataId;
        quantity = qty;
        
        // Clear legacy fields for new saves
        itemName = "";
        itemId = 0;
        maxStackSize = 0;
        spritePath = "";
    }
    
    // Legacy constructor for backward compatibility
    public SerializableInventoryItem(string name, int id, int qty, int maxStack, string spritePath)
    {
        this.itemName = name;
        this.itemId = id;
        this.quantity = qty;
        this.maxStackSize = maxStack;
        this.spritePath = spritePath;
        this.itemDataId = 0; // No ItemData reference for legacy items
    }
    
    public static SerializableInventoryItem FromInventoryItem(InventoryItem item)
    {
        if (item == null) return null;
        
        // Prefer new ID-based serialization
        if (item.ItemId > 0)
        {
            return new SerializableInventoryItem(item.ItemId, item.Quantity);
        }
        
        // Fallback to legacy path if ItemId is invalid
        if (string.IsNullOrEmpty(item.ItemName))
        {
            Debug.LogWarning("Attempting to serialize InventoryItem without valid ID or name. Skipping.");
            return null;
        }
        
        return new SerializableInventoryItem(
            item.ItemName,
            item.ItemId,
            item.Quantity,
            item.MaxStackSize,
            GetSpritePath(item.ItemSprite)
        );
    }
    
    /// <summary>
    /// Create SerializableInventoryItem from ItemData (preferred method for new saves)
    /// </summary>
    public static SerializableInventoryItem FromItemData(ItemData itemData, int quantity)
    {
        if (itemData == null) return null;
        return new SerializableInventoryItem(itemData.itemId, quantity);
    }
    
    public InventoryItem ToInventoryItem()
    {
        // First, attempt ID-based deserialization using ItemDatabase (preferred)
        int idToUse = itemDataId > 0 ? itemDataId : itemId;
        if (idToUse > 0)
        {
            var item = ItemDatabase.CreateInventoryItem(idToUse, quantity);
            if (item != null)
                return item;
        }
        
        // Legacy fallback using sprite path + basic fields
        Sprite sprite = null;
        if (!string.IsNullOrEmpty(spritePath))
        {
            sprite = Resources.Load<Sprite>(spritePath);
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>("Sprites/" + spritePath);
                if (sprite == null)
                {
                    sprite = Resources.Load<Sprite>("Items/" + spritePath);
                }
            }
        }
        
        return new InventoryItem(
            string.IsNullOrEmpty(itemName) ? $"Item {idToUse}" : itemName,
            sprite,
            idToUse,
            quantity,
            maxStackSize > 0 ? maxStackSize : 99
        );
    }
    
    private static string GetSpritePath(Sprite sprite)
    {
        if (sprite == null) return "";
        
        // Improved sprite path detection
        string path = sprite.name;
        
        // Try to get actual asset path if possible
        #if UNITY_EDITOR
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(sprite);
        if (!string.IsNullOrEmpty(assetPath))
        {
            // Extract just the filename without extension for Resources.Load
            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            
            // Remove the Resources/ part if present
            if (assetPath.Contains("Resources/"))
            {
                int resourcesIndex = assetPath.IndexOf("Resources/") + "Resources/".Length;
                string resourcePath = assetPath.Substring(resourcesIndex);
                path = System.IO.Path.GetDirectoryName(resourcePath) + "/" + fileName;
                path = path.Replace("\\", "/").TrimStart('/');
            }
            else
            {
                path = fileName;
            }
        }
        #endif
        
        return path;
    }
} 