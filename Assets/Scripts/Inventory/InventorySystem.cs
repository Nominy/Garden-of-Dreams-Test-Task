using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 20;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotPrefab;
    
    [Header("Click Settings")]
    
    private List<InventoryItem> items = new List<InventoryItem>();
    private List<InventorySlot> slots = new List<InventorySlot>();
    private InventorySlot selectedSlot;
    
    // Events for external systems
    public System.Action<InventoryItem> OnItemAdded;
    public System.Action<InventoryItem> OnItemRemoved;
    public System.Action OnInventoryChanged;
    
    void Start()
    {
        InitializeInventory();
    }
    
    private void InitializeInventory()
    {
        // If slots parent is assigned, get existing slots or create new ones
        if (slotsParent != null)
        {
            // First, try to get existing slots
            InventorySlot[] existingSlots = slotsParent.GetComponentsInChildren<InventorySlot>();
            
            if (existingSlots.Length > 0)
            {
                // Use existing slots
                slots.AddRange(existingSlots);
            }
            else if (slotPrefab != null)
            {
                // Create new slots if none exist
                CreateSlots();
            }
            
            // Initialize all slots
            foreach (var slot in slots)
            {
                slot.Initialize(this);
            }
        }
        
        // Ensure items list matches max slots
        while (items.Count < maxSlots)
        {
            items.Add(null);
        }
    }
    
    private void CreateSlots()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            
            if (slot == null)
            {
                slot = slotObj.AddComponent<InventorySlot>();
            }
            
            slots.Add(slot);
        }
    }
    
    /// <summary>
    /// Public API: Add an item to the inventory
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <returns>True if item was added successfully</returns>
    public bool AddItem(InventoryItem item)
    {
        if (item == null) return false;
        return AddItem(item, item.Quantity);
    }
    
    /// <summary>
    /// Public API: Add an item to the inventory with specific quantity
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <param name="quantity">Amount to add</param>
    /// <returns>True if all items were added successfully</returns>
    public bool AddItem(InventoryItem item, int quantity)
    {
        if (item == null || quantity <= 0) return false;
        
        int remainingToAdd = quantity;
        
        // First, try to stack with existing items
        for (int i = 0; i < items.Count && remainingToAdd > 0; i++)
        {
            if (items[i] != null && items[i].CanStackWith(item) && items[i].HasRoom)
            {
                int overflow = items[i].AddQuantity(remainingToAdd);
                remainingToAdd = overflow;
                
                // Update the slot display
                if (i < slots.Count)
                {
                    slots[i].RefreshDisplay();
                }
            }
        }
        
        // Then, find empty slots for remaining items
        while (remainingToAdd > 0)
        {
            int emptySlotIndex = -1;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    emptySlotIndex = i;
                    break;
                }
            }
            
            if (emptySlotIndex == -1)
            {
                // No more empty slots
                Debug.LogWarning($"Inventory is full! Could not add {remainingToAdd} of item: {item.ItemName}");
                return quantity == remainingToAdd ? false : true; // Return true if we added at least some
            }
            
            // Create new stack in empty slot
            int amountForThisStack = Mathf.Min(remainingToAdd, item.MaxStackSize);
            InventoryItem newStack = new InventoryItem(item.ItemName, item.ItemSprite, item.ItemId, amountForThisStack, item.MaxStackSize);
            
            items[emptySlotIndex] = newStack;
            remainingToAdd -= amountForThisStack;
            
            if (emptySlotIndex < slots.Count)
            {
                slots[emptySlotIndex].SetItem(newStack);
            }
        }
        
        OnItemAdded?.Invoke(item);
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    /// <summary>
    /// Public API: Remove an item from a specific slot
    /// </summary>
    /// <param name="slotIndex">Index of the slot to clear</param>
    /// <returns>The removed item, or null if slot was empty</returns>
    public InventoryItem RemoveItemAt(int slotIndex)
    {
        return RemoveItemAt(slotIndex, -1); // Remove entire stack
    }
    
    /// <summary>
    /// Public API: Remove a specific quantity from a slot
    /// </summary>
    /// <param name="slotIndex">Index of the slot</param>
    /// <param name="quantity">Amount to remove (-1 for entire stack)</param>
    /// <returns>The removed item with the actual quantity removed</returns>
    public InventoryItem RemoveItemAt(int slotIndex, int quantity)
    {
        if (slotIndex < 0 || slotIndex >= items.Count || items[slotIndex] == null)
            return null;
        
        InventoryItem currentItem = items[slotIndex];
        
        if (quantity == -1 || quantity >= currentItem.Quantity)
        {
            // Remove entire stack
            InventoryItem removedItem = new InventoryItem(currentItem);
            items[slotIndex] = null;
            
            if (slotIndex < slots.Count)
            {
                slots[slotIndex].ClearSlot();
            }
            
            // Clear selection if this slot was selected
            if (selectedSlot == slots[slotIndex])
            {
                ClearSelection();
            }
            
            OnItemRemoved?.Invoke(removedItem);
            OnInventoryChanged?.Invoke();
            return removedItem;
        }
        else
        {
            // Remove partial quantity
            int actualRemoved = currentItem.RemoveQuantity(quantity);
            
            if (currentItem.IsEmpty)
            {
                items[slotIndex] = null;
                if (slotIndex < slots.Count)
                {
                    slots[slotIndex].ClearSlot();
                }
                
                if (selectedSlot == slots[slotIndex])
                {
                    ClearSelection();
                }
            }
            else
            {
                // Update display
                if (slotIndex < slots.Count)
                {
                    slots[slotIndex].RefreshDisplay();
                }
            }
            
            InventoryItem removedItem = new InventoryItem(currentItem.ItemName, currentItem.ItemSprite, currentItem.ItemId, actualRemoved, currentItem.MaxStackSize);
            OnItemRemoved?.Invoke(removedItem);
            OnInventoryChanged?.Invoke();
            return removedItem;
        }
    }
    
    /// <summary>
    /// Public API: Check if inventory has space for more items
    /// </summary>
    /// <returns>True if there's at least one empty slot</returns>
    public bool HasSpace()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) return true;
        }
        return false;
    }
    
    /// <summary>
    /// Public API: Get all items in inventory (including null slots)
    /// </summary>
    /// <returns>List of items with null representing empty slots</returns>
    public List<InventoryItem> GetAllItems()
    {
        return new List<InventoryItem>(items);
    }
    
    /// <summary>
    /// Public API: Get only non-null items
    /// </summary>
    /// <returns>List of items excluding empty slots</returns>
    public List<InventoryItem> GetItems()
    {
        List<InventoryItem> nonNullItems = new List<InventoryItem>();
        foreach (var item in items)
        {
            if (item != null)
                nonNullItems.Add(item);
        }
        return nonNullItems;
    }
    
    /// <summary>
    /// Called by InventorySlot when clicked
    /// </summary>
    /// <param name="clickedSlot">The slot that was clicked</param>
    public void OnSlotClicked(InventorySlot clickedSlot)
    {
        if (!clickedSlot.HasItem && selectedSlot == null && !clickedSlot.IsSelected)
        {
            // If the clicked slot is empty and no slot is currently selected,
            // and the clicked slot itself is not marked as selected (e.g. from a previous action),
            // there's nothing to do.
            return;
        }

        if (selectedSlot == clickedSlot)
        {
            // Clicking the already selected slot deselects it.
            ClearSelection();
        }
        else
        {
            // Clicking a new slot selects it.
            SetSelectedSlot(clickedSlot);
        }
    }
    
    private void SetSelectedSlot(InventorySlot slot)
    {
        // Clear previous selection
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
        }
        
        // Set new selection
        selectedSlot = slot;
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(true);
        }
    }
    
    private void ClearSelection()
    {
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
            selectedSlot = null;
        }
    }
    
    public void RequestDeleteItemInSlot(InventorySlot slot)
    {
        if (slot == null || !slots.Contains(slot))
        {
            Debug.LogWarning("RequestDeleteItemInSlot: Slot not found in inventory system.");
            return;
        }

        int slotIndex = slots.IndexOf(slot);
        if (slotIndex != -1)
        {
            RemoveItemAt(slotIndex); // Remove entire stack from this slot
        }
    }
    
    // Public properties for external access
    public int MaxSlots => maxSlots;
    public int UsedSlots => GetItems().Count;
    public InventorySlot SelectedSlot => selectedSlot;
} 