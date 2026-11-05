using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    [Header("Camera Settings")]
    public float distance = 5f;
    public float sensitivity = 0.15f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float cameraHeight = 1.5f;
    [Header("Camera Follow")]
    public float followSpeed = 10f;
    public float rotationFollowSpeed = 8f;
    [Header("Zoom")]
    public float zoomSpeed = 1f;
    [Header("Vertical Rotation")]
    public float minPitch = -30f;
    public float maxPitch = 70f;
    [Header("Camera Collision")]
    public bool enableCameraCollision = true;
    public LayerMask collisionLayers = ~0;
    public float collisionRadius = 0.25f;
    public float collisionOffset = 0.1f;
    public float collisionReturnSpeed = 5f;

    private float yaw;
    private float pitch = 20f;
    private float currentCameraDistance;

    private void Start()
    {
        if (target != null) yaw = target.eulerAngles.y;
        currentCameraDistance = distance;
    }

    private void LateUpdate()
    {
        if (target == null || Mouse.current == null || Keyboard.current == null) return;
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;
        // The inventory owns the cursor while open. Never capture or rotate here.
        bool inventoryOpen = InventoryUI.IsOpen;

        if (!inventoryOpen && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (keyboard.escapeKey.wasPressedThisFrame && !inventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        bool freeLook = keyboard.leftAltKey.isPressed;
        if (!inventoryOpen && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            if (freeLook) yaw += mouseDelta.x * sensitivity;
            else
            {
                target.Rotate(Vector3.up, mouseDelta.x * sensitivity, Space.World);
                yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, rotationFollowSpeed * Time.deltaTime);
            }
            pitch -= mouseDelta.y * sensitivity;
        }
        if (!freeLook && !inventoryOpen)
            yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, rotationFollowSpeed * Time.deltaTime);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (!inventoryOpen)
        {
            float scroll = mouse.scroll.ReadValue().y;
            distance -= (scroll / 120f) * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = target.position + Vector3.up * cameraHeight;
        Vector3 cameraDirection = -(rotation * Vector3.forward);
        float targetCameraDistance = distance;
        if (enableCameraCollision && Physics.SphereCast(focusPoint, collisionRadius, cameraDirection,
            out RaycastHit hit, distance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            targetCameraDistance = Mathf.Max(0f, hit.distance - collisionOffset);
        }
        if (targetCameraDistance < currentCameraDistance) currentCameraDistance = targetCameraDistance;
        else currentCameraDistance = Mathf.Lerp(currentCameraDistance, targetCameraDistance,
            collisionReturnSpeed * Time.deltaTime);
        Vector3 desiredPosition = focusPoint + cameraDirection * currentCameraDistance;
        if (targetCameraDistance < distance) transform.position = desiredPosition;
        else transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.rotation = rotation;
    }
}
