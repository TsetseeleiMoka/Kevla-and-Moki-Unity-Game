using UnityEngine;

public enum EquipmentSlot { None, Head, Chest, Legs, Feet, Hands, Face, Back }

[CreateAssetMenu(fileName = "NewItem", menuName = "Survival Game/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "Item";
    [TextArea] public string description;
    public Sprite icon;
    [Min(1)] public int maxStack = 50;
    public EquipmentSlot equipmentSlot = EquipmentSlot.None;
    public WorldItemPickup worldPrefab;
}
