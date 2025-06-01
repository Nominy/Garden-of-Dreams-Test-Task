using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Enemy Loot Table", menuName = "Game/Enemy Loot Table")]
public class EnemyLootTable : ScriptableObject
{
    [System.Serializable]
    public class LootEntry
    {
        [Header("Item Data")]
        public ItemData itemData; // Direct reference to ItemData ScriptableObject
        
        [Header("Drop Settings")]
        [Range(0f, 100f)]
        public float dropChance = 50f; // Percentage chance to drop
        
        [Header("Quantity")]
        public int minQuantity = 1;
        public int maxQuantity = 1;
        
        [Header("Conditions")]
        public bool guaranteedDrop = false; // Always drops regardless of chance
        
        public InventoryItem CreateItem()
        {
            if (itemData == null)
            {
                Debug.LogError("LootEntry: ItemData is not assigned!");
                return null;
            }

            int quantity = Random.Range(minQuantity, maxQuantity + 1);
            return itemData.CreateInventoryItem(quantity);
        }
        
        public bool ShouldDrop()
        {
            if (guaranteedDrop) return true;
            return Random.Range(0f, 100f) <= dropChance;
        }
    }
    
    [Header("Loot Entries")]
    [SerializeField] private List<LootEntry> lootEntries = new List<LootEntry>();
    
    public List<InventoryItem> GetLootDrops()
    {
        List<InventoryItem> droppedItems = new List<InventoryItem>();
        List<LootEntry> availableEntries = new List<LootEntry>(lootEntries);
        
        // First, handle guaranteed drops
        var guaranteedEntries = availableEntries.Where(entry => entry.guaranteedDrop).ToList();
        foreach (var entry in guaranteedEntries)
        {
            var item = entry.CreateItem();
            if (item != null)
            {
                droppedItems.Add(item);
            }
            availableEntries.Remove(entry);
        }
        
        // Then, handle chance-based drops
        var chanceEntries = availableEntries.Where(entry => !entry.guaranteedDrop).ToList();
        
        // Shuffle the entries for random order
        for (int i = 0; i < chanceEntries.Count; i++)
        {
            var temp = chanceEntries[i];
            int randomIndex = Random.Range(i, chanceEntries.Count);
            chanceEntries[i] = chanceEntries[randomIndex];
            chanceEntries[randomIndex] = temp;
        }
        
        foreach (var entry in chanceEntries)
        {
            if (entry.ShouldDrop())
            {
                var item = entry.CreateItem();
                if (item != null)
                {
                    droppedItems.Add(item);
                }
                availableEntries.Remove(entry);
            }
        }
        
        return droppedItems;
    }
    
    public void AddLootEntry(LootEntry entry)
    {
        if (entry != null && !lootEntries.Contains(entry))
        {
            lootEntries.Add(entry);
        }
    }
    
    public void RemoveLootEntry(LootEntry entry)
    {
        lootEntries.Remove(entry);
    }
    
    public List<LootEntry> GetAllEntries()
    {
        return new List<LootEntry>(lootEntries);
    }
    
    public float GetTotalDropChance()
    {
        return lootEntries.Sum(entry => entry.dropChance);
    }
    
    public int GetGuaranteedDropCount()
    {
        return lootEntries.Count(entry => entry.guaranteedDrop);
    }
    
    // For debugging and preview
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    void OnValidate()
    {
        if (showDebugInfo)
        {
            Debug.Log($"Loot Table: {name}");
            Debug.Log($"Total Entries: {lootEntries.Count}");
            Debug.Log($"Guaranteed Drops: {GetGuaranteedDropCount()}");
            Debug.Log($"Total Drop Chance: {GetTotalDropChance():F1}%");
        }
        
        // Validate all entries
        foreach (var entry in lootEntries)
        {
            if (entry.itemData == null)
            {
                Debug.LogWarning($"Loot Table '{name}' has an entry with no ItemData assigned!", this);
            }
            
            if (entry.minQuantity <= 0)
                entry.minQuantity = 1;
                
            if (entry.maxQuantity < entry.minQuantity)
                entry.maxQuantity = entry.minQuantity;
        }
    }
    
    // Test method to simulate drops (for debugging in editor)
    [ContextMenu("Test Loot Drop")]
    public void TestLootDrop()
    {
        var drops = GetLootDrops();
        Debug.Log($"Test drop from {name}: {drops.Count} items dropped");
        foreach (var item in drops)
        {
            if (item != null)
            {
                Debug.Log($"- {item.ItemName} x{item.Quantity}");
            }
        }
    }
} 