using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class InteractableCube : Interactable
{
    public Color normalColor = Color.white;
    public Color interactedColor = Color.green;
    private Material cubeMaterial;
    private bool activated;

    private void Awake()
    {
        cubeMaterial = GetComponent<Renderer>().material;
        cubeMaterial.color = normalColor;
    }

    public override string GetInteractionText() => activated ? "Deactivate" : "Activate";

    public override void Interact(GameObject player)
    {
        activated = !activated;
        cubeMaterial.color = activated ? interactedColor : normalColor;
        Debug.Log(activated ? "Cube activated!" : "Cube deactivated!");
    }
}
