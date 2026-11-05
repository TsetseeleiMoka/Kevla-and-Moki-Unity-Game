using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public enum InventoryArea { Main, Hotbar, Clothing, Loot }

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public TMP_Text quantityText;
    public TMP_Text nameText;
    public InventoryArea area;
    public int index;
    private InventoryUI owner;

    public void Setup(InventoryUI ui, InventoryArea slotArea, int slotIndex)
    {
        owner = ui; area = slotArea; index = slotIndex;
        Refresh();
    }

    public void Refresh()
    {
        if (owner == null) return;
        InventoryContainer store = owner.GetContainer(area);
        if (store == null || index >= store.Count) return;
        InventoryStack stack = store.Get(index);
        if (icon != null)
        {
            icon.enabled = !stack.Empty && stack.item.icon != null;
            if (!stack.Empty) icon.sprite = stack.item.icon;
            icon.raycastTarget = false;
        }
        if (quantityText != null)
        {
            quantityText.text = stack.Empty || stack.quantity <= 1 ? "" : stack.quantity.ToString();
            quantityText.raycastTarget = false;
        }
        if (nameText != null)
        {
            nameText.text = stack.Empty ? "" : stack.item.itemName;
            nameText.raycastTarget = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (owner == null) return;
        if (eventData.button == PointerEventData.InputButton.Right)
            owner.RightClick(area, index);
        else if (eventData.button == PointerEventData.InputButton.Left)
            owner.LeftClick(area, index, UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed);
    }

    public void OnBeginDrag(PointerEventData e) { if (e.button == PointerEventData.InputButton.Left) owner?.BeginDrag(area, index); }
    public void OnDrag(PointerEventData e) { if (e.button == PointerEventData.InputButton.Left) owner?.MoveCursor(e.position); }
    public void OnDrop(PointerEventData e) { owner?.DropOn(area, index); }
    public void OnEndDrag(PointerEventData e) { if (e.button == PointerEventData.InputButton.Left) owner?.EndDrag(); }
}
