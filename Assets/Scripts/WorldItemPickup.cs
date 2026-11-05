using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldItemPickup : Interactable
{
    public ItemData item;
    [Min(1)] public int amount = 1;

    public override string GetInteractionText() =>
        item == null ? "Unconfigured pickup" : "Pick up " + item.itemName + " x" + amount;

    public override void Interact(GameObject player)
    {
        if (item == null) { Debug.LogWarning("Assign an item to " + name); return; }
        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null) { Debug.LogWarning("PlayerInventory missing"); return; }
        int collected = inventory.AddItem(item, amount);
        if (collected == 0) { Debug.Log("Inventory full"); return; }
        amount -= collected;
        if (amount <= 0) Destroy(gameObject);
    }
}
