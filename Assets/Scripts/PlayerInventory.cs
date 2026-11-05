using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventoryContainer main = new InventoryContainer();
    public InventoryContainer hotbar = new InventoryContainer();
    public InventoryContainer clothing = new InventoryContainer();

    private void Awake()
    {
        main.Initialize(24);
        hotbar.Initialize(6);
        clothing.Initialize(7);
    }

    // Pickups fill existing main stacks, then empty main slots, then hotbar.
    public int AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return 0;
        int added = main.Add(item, amount);
        if (added < amount) added += hotbar.Add(item, amount - added);
        return added;
    }
}
