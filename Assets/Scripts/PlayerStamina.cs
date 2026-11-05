using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [Min(1f)] public float maxStamina = 100f;
    [Min(0f)] public float staminaRegenRate = 12f;
    [Min(0f)] public float staminaRegenDelay = 1.25f;
    [Header("Movement Costs")]
    [Min(0f)] public float sprintStaminaCost = 15f;
    [Min(0f)] public float jumpStaminaCost = 15f;
    [Header("Exhaustion")]
    [Min(0f)] public float exhaustionRecovery = 20f;
    [Header("Optional UI")]
    public Image staminaFillImage;

    public float CurrentStamina { get; private set; }
    public bool IsExhausted { get; private set; }
    public float StaminaPercentage => CurrentStamina / Mathf.Max(1f, maxStamina);
    private float lastStaminaUseTime;

    private void Awake()
    {
        maxStamina = Mathf.Max(1f, maxStamina);
        CurrentStamina = maxStamina;
        lastStaminaUseTime = -staminaRegenDelay;
        UpdateStaminaUI();
    }

    private void Update()
    {
        if (CurrentStamina < maxStamina && Time.time - lastStaminaUseTime >= staminaRegenDelay)
        {
            CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + staminaRegenRate * Time.deltaTime);
            if (IsExhausted && CurrentStamina >= Mathf.Min(exhaustionRecovery, maxStamina))
                IsExhausted = false;
        }
        UpdateStaminaUI();
    }

    public bool CanSprint() => !IsExhausted && CurrentStamina > 0f;

    public bool TryUseStamina(float amount)
    {
        if (amount <= 0f) return true;
        if (IsExhausted || CurrentStamina < amount) return false;
        UseStamina(amount);
        return true;
    }

    public void UseStamina(float amount)
    {
        if (amount <= 0f) return;
        CurrentStamina = Mathf.Max(0f, CurrentStamina - amount);
        lastStaminaUseTime = Time.time;
        if (CurrentStamina <= 0f) IsExhausted = true;
        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        if (staminaFillImage != null)
            staminaFillImage.fillAmount = StaminaPercentage;
    }
}
