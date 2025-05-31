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
            
            // Update quantity text
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
        else
        {
            ClearSlot();
        }
        
        UpdateVisuals();
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
        
        SetSelected(false);
        UpdateVisuals();
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();
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
} 