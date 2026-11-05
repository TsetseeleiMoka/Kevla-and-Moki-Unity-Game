using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LootContainer : Interactable
{
    [Min(1)] public int numberOfSlots = 18;
    public InventoryContainer contents = new InventoryContainer();

    private void Awake() => contents.Initialize(numberOfSlots);
    public override string GetInteractionText() => "Open " + gameObject.name;
    public override void Interact(GameObject player)
    {
        if (InventoryUI.Instance == null) { Debug.LogWarning("InventoryUI missing"); return; }
        InventoryUI.Instance.OpenLoot(this);
    }
}
