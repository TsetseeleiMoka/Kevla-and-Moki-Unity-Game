using UnityEngine;

[RequireComponent(typeof(LootContainer))]
public class DemoChestContents : MonoBehaviour
{
    public ItemData starterItem;
    [Min(1)] public int quantity = 17;
    private void Start()
    {
        if (starterItem != null)
            GetComponent<LootContainer>().contents.Add(starterItem, quantity);
    }
}
