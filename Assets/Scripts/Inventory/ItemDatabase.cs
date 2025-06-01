using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralised database for all ItemData assets at runtime.
/// Allows lookup by itemId so save system can restore items without sprite paths or Resources dependency.
/// </summary>
public class ItemDatabase : MonoBehaviour
{
    private static Dictionary<int, ItemData> itemDataById = new Dictionary<int, ItemData>();
    public static ItemDatabase Instance { get; private set; }

    [Tooltip("Optional manual list of ItemData. If empty, all ItemData assets will be loaded from Resources.")]
    [SerializeField] private ItemData[] itemDataAssets;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // If no assets assigned manually, try load all ItemData from Resources
        if (itemDataAssets == null || itemDataAssets.Length == 0)
        {
            itemDataAssets = Resources.LoadAll<ItemData>("");
        }

        foreach (var data in itemDataAssets)
        {
            if (data == null) continue;
            if (itemDataById.ContainsKey(data.itemId))
            {
                Debug.LogWarning($"ItemDatabase: Duplicate itemId {data.itemId} detected ( {data.itemName} ). Only first instance will be used.");
                continue;
            }
            itemDataById.Add(data.itemId, data);
        }

        Debug.Log($"ItemDatabase initialised with {itemDataById.Count} items.");
    }

    /// <summary>
    /// Get ItemData by numeric id.
    /// </summary>
    public static ItemData GetItemData(int id)
    {
        if (itemDataById.TryGetValue(id, out var data))
            return data;
        return null;
    }

    /// <summary>
    /// Create an InventoryItem from id and quantity, if ItemData exists; otherwise returns null.
    /// </summary>
    public static InventoryItem CreateInventoryItem(int id, int qty = 1)
    {
        var data = GetItemData(id);
        if (data != null)
            return data.CreateInventoryItem(qty);
        Debug.LogWarning($"ItemDatabase: ItemData with id {id} not found. Returning null.");
        return null;
    }
} 