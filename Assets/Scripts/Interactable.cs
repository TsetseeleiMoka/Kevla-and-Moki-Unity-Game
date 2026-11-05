using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    public string interactionName = "Interact";

    public virtual string GetInteractionText() => interactionName;
    public abstract void Interact(GameObject player);
}
