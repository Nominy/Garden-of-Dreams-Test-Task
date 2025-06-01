using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Button deleteButton;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalBackgroundColor = Color.white;
    [SerializeField] private Color selectedBackgroundColor = Color.yellow;
    [SerializeField] private Color emptySlotColor = Color.clear;
    
    private InventoryItem currentItem;
    private bool isSelected = false;
    private InventorySystem inventorySystem;
    
    public bool HasItem => currentItem != null;
    public bool IsSelected => isSelected;
    public InventoryItem CurrentItem => currentItem;
    
    public void Initialize(InventorySystem inventory)
    {
        inventorySystem = inventory;
        
        if (itemImage == null)
            itemImage = GetComponentInChildren<Image>();
        
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        
        if (quantityText == null)
            quantityText = GetComponentInChildren<TextMeshProUGUI>();

        // Removed programmatic listener setup for deleteButton.
        // Please assign OnDeleteButtonPressed to the button's OnClick event in the Unity Inspector.
        
        ClearSlot();
    }
    
    public void SetItem(InventoryItem item)
    {
        currentItem = item;
        RefreshDisplay();
    }
    
    public void RefreshDisplay()
    {
        if (currentItem != null && itemImage != null)
        {
            itemImage.sprite = currentItem.ItemSprite;
            itemImage.color = Color.white;
            itemImage.enabled = true;
            
            if (quantityText != null)
            {
                if (currentItem.Quantity > 1)
                {
                    quantityText.text = currentItem.Quantity.ToString();
                    quantityText.enabled = true;
                }
                else
                {
                    quantityText.enabled = false;
                }
            }
        }
        else // currentItem is null
        {
            // Logic for clearing the visual elements of the slot when item is null
            // This is mostly covered by ClearSlot, but we need to handle image and text specifically here
            // if they weren't cleared by an explicit ClearSlot() call before a refresh.
            if (itemImage != null)
            {
                itemImage.sprite = null;
                itemImage.color = emptySlotColor;
                itemImage.enabled = false;
            }
            
            if (quantityText != null)
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }
            // If currentItem is null, it's effectively an empty slot, so ensure selection logic reflects this.
            // If it was selected, and item removed, it should behave like an empty selected slot.
        }
        
        // Update visuals for background and delete button visibility
        UpdateVisuals(); 
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(isSelected && HasItem);
        }
    }
    
    public void ClearSlot()
    {
        currentItem = null;
        
        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.color = emptySlotColor;
            itemImage.enabled = false;
        }
        
        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.enabled = false;
        }
        
        SetSelected(false); // This will also hide delete button and call UpdateVisuals()
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();

        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(isSelected && HasItem);
        }
    }
    
    private void UpdateVisuals()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedBackgroundColor : normalBackgroundColor;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventorySystem != null)
        {
            inventorySystem.OnSlotClicked(this);
        }
    }

    public void OnDeleteButtonPressed()
    {
        if (inventorySystem != null && HasItem)
        {
            inventorySystem.RequestDeleteItemInSlot(this);
            // InventorySystem should then call ClearSlot() on this slot
        }
    }
} 