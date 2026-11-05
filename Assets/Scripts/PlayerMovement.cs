using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStamina))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    [Header("Jump and Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    private CharacterController controller;
    private PlayerStamina stamina;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stamina = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        // Keep inventory keyboard interaction from moving the player.
        if (InventoryUI.IsOpen || Keyboard.current == null) return;

        Keyboard keyboard = Keyboard.current;
        float x = 0f, z = 0f;
        if (keyboard.wKey.isPressed) z += 1f;
        if (keyboard.sKey.isPressed) z -= 1f;
        if (keyboard.aKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed) x += 1f;

        Vector3 localInput = new Vector3(x, 0f, z).normalized;
        Vector3 direction = transform.forward * localInput.z + transform.right * localInput.x;
        bool wantsSprint = keyboard.leftShiftKey.isPressed && localInput.sqrMagnitude > 0.01f;
        bool sprinting = wantsSprint && stamina.CanSprint();

        if (sprinting)
            stamina.UseStamina(stamina.sprintStaminaCost * Time.deltaTime);

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (keyboard.spaceKey.wasPressedThisFrame && controller.isGrounded &&
            stamina.TryUseStamina(stamina.jumpStaminaCost))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 motion = direction * (sprinting ? sprintSpeed : walkSpeed);
        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);
    }
}
