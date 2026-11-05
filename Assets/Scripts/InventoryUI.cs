using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }
    public static bool IsOpen => Instance != null && Instance.open;

    [Header("References")]
    public PlayerInventory playerInventory;
    public GameObject inventoryPanel;
    public GameObject lootPanel;
    public Transform mainGrid;
    public Transform hotbarGrid;
    public Transform clothingGrid;
    public Transform lootGrid;
    public InventorySlotUI slotPrefab;
    public Image heldIcon;
    public TMP_Text heldQuantity;
    public Camera worldCamera;

    [Header("World Dropping")]
    public float dropDistance = 2f;

    private bool open;
    private LootContainer activeLoot;
    private InventoryStack held;
    private InventoryContainer origin;
    private int originIndex = -1;
    private bool dragEndedOnSlot;
    private readonly List<InventorySlotUI> views = new List<InventorySlotUI>();
    private readonly List<InventorySlotUI> lootViews = new List<InventorySlotUI>();

    private void Awake()
    {
        Instance = this;
        if (worldCamera == null) worldCamera = Camera.main;
        if (playerInventory == null) Debug.LogError("Assign PlayerInventory in InventoryUI");
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (lootPanel != null) lootPanel.SetActive(false);
        SetHeldVisual();
    }

    private void Start()
    {
        BuildGrid(mainGrid, InventoryArea.Main, 24, views);
        BuildGrid(hotbarGrid, InventoryArea.Hotbar, 6, views);
        BuildGrid(clothingGrid, InventoryArea.Clothing, 7, views);
        RefreshAll();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (open) Close(); else Open();
        }
        if (open && held != null && Mouse.current != null)
            MoveCursor(Mouse.current.position.ReadValue());
    }

    private void LateUpdate()
    {
        if (open) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }

    public InventoryContainer GetContainer(InventoryArea area)
    {
        if (playerInventory == null) return null;
        switch (area)
        {
            case InventoryArea.Main: return playerInventory.main;
            case InventoryArea.Hotbar: return playerInventory.hotbar;
            case InventoryArea.Clothing: return playerInventory.clothing;
            case InventoryArea.Loot: return activeLoot == null ? null : activeLoot.contents;
            default: return null;
        }
    }

    private void BuildGrid(Transform parent, InventoryArea area, int count, List<InventorySlotUI> list)
    {
        if (parent == null || slotPrefab == null) { Debug.LogError("Missing grid or slot prefab: " + area); return; }
        for (int i = 0; i < count; i++)
        {
            InventorySlotUI view = Instantiate(slotPrefab, parent);
            view.gameObject.SetActive(true);
            view.Setup(this, area, i);
            list.Add(view);
        }
    }

    public void RefreshAll()
    {
        foreach (var view in views) view.Refresh();
        foreach (var view in lootViews) view.Refresh();
        SetHeldVisual();
    }

    private void Open()
    {
        open = true;
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        RefreshAll();
    }

    public void OpenLoot(LootContainer container)
    {
        if (container == null) return;
        ReturnHeld();
        activeLoot = container;
        foreach (var view in lootViews) if (view != null) Destroy(view.gameObject);
        lootViews.Clear();
        BuildGrid(lootGrid, InventoryArea.Loot, container.contents.Count, lootViews);
        if (lootPanel != null) lootPanel.SetActive(true);
        Open();
    }

    public void Close()
    {
        ReturnHeld();
        open = false;
        activeLoot = null;
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (lootPanel != null) lootPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool CanAccept(InventoryArea area, int index, ItemData item)
    {
        if (item == null) return false;
        if (area != InventoryArea.Clothing) return true;
        return item.equipmentSlot != EquipmentSlot.None && (int)item.equipmentSlot == index + 1;
    }

    private void Take(InventoryArea area, int index, bool half)
    {
        var store = GetContainer(area);
        if (store == null || index >= store.Count) return;
        var slot = store.Get(index);
        if (slot.Empty) return;
        int n = half ? (slot.quantity + 1) / 2 : slot.quantity;
        held = new InventoryStack(slot.item, n);
        origin = store;
        originIndex = index;
        slot.quantity -= n;
        if (slot.quantity == 0) slot.Clear();
        store.Notify();
        RefreshAll();
    }

    private bool Place(InventoryArea area, int index, bool one)
    {
        if (held == null || held.Empty) return false;
        InventoryContainer store = GetContainer(area);
        if (store == null || index < 0 || index >= store.Count) return false;
        if (!CanAccept(area, index, held.item)) return false;
        InventoryStack slot = store.Get(index);
        if (slot.Empty)
        {
            int n = one ? 1 : held.quantity;
            slot.item = held.item;
            slot.quantity = n;
            held.quantity -= n;
        }
        else if (slot.item == held.item)
        {
            int space = Mathf.Max(0, held.item.maxStack - slot.quantity);
            int n = Mathf.Min(one ? 1 : held.quantity, space);
            if (n == 0) return false;
            slot.quantity += n;
            held.quantity -= n;
        }
        else if (!one && CanAcceptOriginForSwap(slot.item))
        {
            var displaced = slot.Copy();
            slot.item = held.item; slot.quantity = held.quantity;
            held = displaced;
            // The previous destination is now the origin for returning a displaced stack.
            origin = store; originIndex = index;
        }
        else return false;

        if (held != null && held.quantity <= 0) { held = null; origin = null; originIndex = -1; }
        store.Notify();
        RefreshAll();
        return true;
    }

    private bool CanAcceptOriginForSwap(ItemData displaced)
    {
        // Swapping with a clothing slot is safe only if the displaced item
        // can be carried elsewhere by the cursor (and later placed manually).
        return displaced != null;
    }

    public void LeftClick(InventoryArea area, int index, bool shift)
    {
        if (!open) return;
        if (shift && held == null) { QuickTransfer(area, index); return; }
        if (held == null) Take(area, index, false);
        else Place(area, index, false);
    }

    public void RightClick(InventoryArea area, int index)
    {
        if (!open) return;
        if (held == null) Take(area, index, true);
        else Place(area, index, true);
    }

    public void BeginDrag(InventoryArea area, int index)
    {
        if (!open) return;
        dragEndedOnSlot = false;
        if (held == null) Take(area, index, false);
    }

    public void DropOn(InventoryArea area, int index)
    {
        if (!open || held == null) return;
        dragEndedOnSlot = true;
        Place(area, index, false);
    }

    public void EndDrag()
    {
        if (held == null) return;
        if (!dragEndedOnSlot) DropHeldToWorld();
        dragEndedOnSlot = false;
    }

    private void QuickTransfer(InventoryArea sourceArea, int index)
    {
        InventoryContainer source = GetContainer(sourceArea);
        if (source == null || index < 0 || index >= source.Count) return;
        var slot = source.Get(index);
        if (slot.Empty) return;
        InventoryContainer destination;
        if (sourceArea == InventoryArea.Loot) destination = playerInventory.main;
        else if (activeLoot != null) destination = activeLoot.contents;
        else if (sourceArea == InventoryArea.Main) destination = playerInventory.hotbar;
        else destination = playerInventory.main;
        if (destination == source) return;
        int moved = destination.Add(slot.item, slot.quantity);
        slot.quantity -= moved;
        if (slot.quantity <= 0) slot.Clear();
        source.Notify(); RefreshAll();
    }

    private void ReturnHeld()
    {
        if (held == null || held.Empty) return;
        if (origin != null && originIndex >= 0 && originIndex < origin.Count)
        {
            InventoryStack slot = origin.Get(originIndex);
            if (slot.Empty) { slot.item = held.item; slot.quantity = held.quantity; held = null; }
            else if (slot.item == held.item)
            {
                int n = Mathf.Min(held.quantity, Mathf.Max(0, held.item.maxStack - slot.quantity));
                slot.quantity += n; held.quantity -= n;
            }
            origin.Notify();
        }
        if (held != null && !held.Empty)
        {
            int n = playerInventory.AddItem(held.item, held.quantity);
            held.quantity -= n;
            if (held.quantity == 0) held = null;
        }
        if (held != null && !held.Empty) DropHeldToWorld();
        origin = null; originIndex = -1;
        RefreshAll();
    }

    public void DropHeldToWorld()
    {
        if (held == null || held.Empty) return;
        if (held.item.worldPrefab == null)
        {
            Debug.LogWarning("Assign World Prefab to item before dropping it: " + held.item.itemName);
            ReturnToInventoryOnly();
            return;
        }
        Transform player = playerInventory.transform;
        Vector3 pos = player.position + player.forward * dropDistance + Vector3.up;
        var pickup = Instantiate(held.item.worldPrefab, pos, Quaternion.identity);
        pickup.item = held.item;
        pickup.amount = held.quantity;
        held = null; origin = null; originIndex = -1;
        RefreshAll();
    }

    private void ReturnToInventoryOnly()
    {
        // Never delete items if a world prefab hasn't been configured.
        if (origin != null && originIndex >= 0 && originIndex < origin.Count)
        {
            var slot = origin.Get(originIndex);
            if (slot.Empty) { slot.item = held.item; slot.quantity = held.quantity; held = null; origin.Notify(); }
        }
        if (held != null && !held.Empty)
        {
            int moved = playerInventory.AddItem(held.item, held.quantity);
            held.quantity -= moved;
            if (held.quantity <= 0) held = null;
        }
        if (held != null && !held.Empty)
            Debug.LogWarning("Held stack retained; make inventory space or assign worldPrefab.");
        RefreshAll();
    }

    public void MoveCursor(Vector2 screenPosition)
    {
        if (heldIcon == null) return;
        heldIcon.rectTransform.position = screenPosition;
        if (heldQuantity != null) heldQuantity.rectTransform.position = screenPosition + new Vector2(25f, -22f);
    }

    private void SetHeldVisual()
    {
        bool show = open && held != null && !held.Empty;
        if (heldIcon != null)
        {
            heldIcon.enabled = show && held.item.icon != null;
            heldIcon.raycastTarget = false;
            if (show) heldIcon.sprite = held.item.icon;
        }
        if (heldQuantity != null)
        {
            heldQuantity.text = show ? held.quantity.ToString() : "";
            heldQuantity.raycastTarget = false;
        }
    }
}
