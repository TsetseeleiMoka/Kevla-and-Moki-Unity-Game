using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerTargeting : MonoBehaviour
{
    public Camera playerCamera;
    public Image crosshair;
    public TMP_Text interactionPrompt;
    public float detectionDistance = 10f;
    public float interactionRange = 3f;
    public LayerMask interactionLayers = ~0;
    public bool combatMode = false;

    private Interactable currentInteractable;
    private bool lookingAtInteractable;
    private bool inRange;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        UpdateUI();
    }
    private void Update()
    {
        if (InventoryUI.IsOpen)
        {
            if (interactionPrompt != null) interactionPrompt.gameObject.SetActive(false);
            if (crosshair != null) crosshair.enabled = false;
            return;
        }
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            combatMode = !combatMode;
        DetectTarget();
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame &&
            currentInteractable != null && inRange)
            currentInteractable.Interact(gameObject);
        UpdateUI();
    }
    private void DetectTarget()
    {
        currentInteractable = null;
        lookingAtInteractable = false;
        inRange = false;
        if (playerCamera == null) return;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, detectionDistance, interactionLayers,
            QueryTriggerInteraction.Ignore))
        {
            var interactable = hit.collider.GetComponentInParent<Interactable>();
            if (interactable == null) return;
            currentInteractable = interactable;
            lookingAtInteractable = true;
            inRange = Vector3.Distance(transform.position,
                hit.collider.ClosestPoint(transform.position)) <= interactionRange;
        }
    }
    private void UpdateUI()
    {
        if (crosshair != null) crosshair.enabled = combatMode || lookingAtInteractable;
        if (interactionPrompt == null) return;
        bool show = currentInteractable != null && inRange;
        interactionPrompt.gameObject.SetActive(show);
        if (show) interactionPrompt.text = "[E] " + currentInteractable.GetInteractionText();
    }
}
