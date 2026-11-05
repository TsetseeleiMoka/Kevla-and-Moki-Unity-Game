using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryStack
{
    public ItemData item;
    public int quantity;
    public bool Empty => item == null || quantity <= 0;
    public InventoryStack() { }
    public InventoryStack(ItemData item, int quantity) { this.item = item; this.quantity = quantity; }
    public InventoryStack Copy() => new InventoryStack(item, quantity);
    public void Clear() { item = null; quantity = 0; }
}

[Serializable]
public class InventoryContainer
{
    [SerializeField] private List<InventoryStack> slots = new List<InventoryStack>();
    public event Action Changed;
    public int Count => slots.Count;
    public InventoryStack Get(int i) => slots[i];
    public void Initialize(int size)
    {
        slots.Clear();
        for (int i = 0; i < size; i++) slots.Add(new InventoryStack());
        Notify();
    }
    public void Notify() => Changed?.Invoke();

    // Adds as many as will fit; returns number added.
    public int Add(ItemData item, int quantity)
    {
        if (item == null || quantity <= 0) return 0;
        int original = quantity;
        int maximum = Mathf.Max(1, item.maxStack);
        foreach (var slot in slots)
        {
            if (slot.Empty || slot.item != item) continue;
            int n = Mathf.Min(quantity, Mathf.Max(0, maximum - slot.quantity));
            slot.quantity += n; quantity -= n;
            if (quantity == 0) break;
        }
        foreach (var slot in slots)
        {
            if (quantity == 0) break;
            if (!slot.Empty) continue;
            int n = Mathf.Min(quantity, maximum);
            slot.item = item; slot.quantity = n; quantity -= n;
        }
        if (quantity != original) Notify();
        return original - quantity;
    }
}
