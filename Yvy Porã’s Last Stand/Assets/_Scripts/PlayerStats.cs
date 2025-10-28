// 28/10/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Atributos do Jogador")]
    public Stat maxHealth;
    public Stat moveSpeed;
    public Stat sprintSpeed;
    public Stat spearDamage;
    public Stat maxAmmo;
    public Stat cooldownReduction;

    private PlayerHealth health;
    private StarterAssets.FirstPersonController controller;
    private Gun gun;
    private Spear spear;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        controller = GetComponent<StarterAssets.FirstPersonController>();
        gun = GetComponentInChildren<Gun>();
        spear = GetComponentInChildren<Spear>();

        ResetAllStats(); // Inicializa os valores base de todos os atributos
        ApplyStats();    // Aplica os atributos ao jogador
    }

    public void ResetAllStats()
    {
        maxHealth.Reset();
        moveSpeed.Reset();
        sprintSpeed.Reset();
        spearDamage.Reset();
        maxAmmo.Reset();
        cooldownReduction.Reset();
    }

    public void ApplyStats()
    {
        if (health != null)
            health.SetMaxHealth(Mathf.RoundToInt(maxHealth.currentValue));

        if (controller != null)
        {
            controller.MoveSpeed = moveSpeed.currentValue;
            controller.SprintSpeed = sprintSpeed.currentValue;
        }

        if (spear != null)
            spear.damage = Mathf.RoundToInt(spearDamage.currentValue);

        if (gun != null)
        {
            gun.maxAmmo = Mathf.RoundToInt(maxAmmo.currentValue);
            gun.cooldownTime *= (1f - cooldownReduction.currentValue);
        }
    }

    public void AddToStat(string statName, float value)
    {
        switch (statName)
        {
            case "MaxHealth":
                maxHealth.Add(value);
                if (health != null)
                    health.SetMaxHealth(Mathf.RoundToInt(maxHealth.currentValue));
                break;
            case "MoveSpeed":
                moveSpeed.Add(value);
                if (controller != null)
                    controller.MoveSpeed = moveSpeed.currentValue;
                break;
            case "SprintSpeed":
                sprintSpeed.Add(value);
                if (controller != null)
                    controller.SprintSpeed = sprintSpeed.currentValue;
                break;
            case "SpearDamage":
                spearDamage.Add(value);
                if (spear != null)
                    spear.damage = Mathf.RoundToInt(spearDamage.currentValue);
                break;
            case "MaxAmmo":
                maxAmmo.Add(value);
                if (gun != null)
                    gun.maxAmmo = Mathf.RoundToInt(maxAmmo.currentValue);
                break;
            case "CooldownReduction":
                cooldownReduction.Add(value);
                if (gun != null)
                    gun.cooldownTime *= (1f - cooldownReduction.currentValue);
                break;
            default:
                Debug.LogWarning($"Atributo desconhecido: {statName}");
                break;
        }
    }

    public void MultiplyStat(string statName, float multiplier)
    {
        switch (statName)
        {
            case "MaxHealth":
                maxHealth.Multiply(multiplier);
                if (health != null)
                    health.SetMaxHealth(Mathf.RoundToInt(maxHealth.currentValue));
                break;
            case "MoveSpeed":
                moveSpeed.Multiply(multiplier);
                if (controller != null)
                    controller.MoveSpeed = moveSpeed.currentValue;
                break;
            case "SprintSpeed":
                sprintSpeed.Multiply(multiplier);
                if (controller != null)
                    controller.SprintSpeed = sprintSpeed.currentValue;
                break;
            case "SpearDamage":
                spearDamage.Multiply(multiplier);
                if (spear != null)
                    spear.damage = Mathf.RoundToInt(spearDamage.currentValue);
                break;
            case "MaxAmmo":
                maxAmmo.Multiply(multiplier);
                if (gun != null)
                    gun.maxAmmo = Mathf.RoundToInt(maxAmmo.currentValue);
                break;
            case "CooldownReduction":
                cooldownReduction.Multiply(multiplier);
                if (gun != null)
                    gun.cooldownTime *= (1f - cooldownReduction.currentValue);
                break;
            default:
                Debug.LogWarning($"Atributo desconhecido: {statName}");
                break;
        }
    }

    public void ApplyCardEffect(string effectType, float value)
    {
        switch (effectType)
        {
            case "MaxHealth":
                maxHealth.Add(value);
                if (health != null)
                    health.SetMaxHealth(Mathf.RoundToInt(maxHealth.currentValue));
                break;
            case "MoveSpeed":
                moveSpeed.Add(value);
                if (controller != null)
                    controller.MoveSpeed = moveSpeed.currentValue;
                break;
            case "SprintSpeed":
                sprintSpeed.Add(value);
                if (controller != null)
                    controller.SprintSpeed = sprintSpeed.currentValue;
                break;
            case "SpearDamage":
                spearDamage.Add(value);
                if (spear != null)
                    spear.damage = Mathf.RoundToInt(spearDamage.currentValue);
                break;
            case "MaxAmmo":
                maxAmmo.Add(value);
                if (gun != null)
                    gun.maxAmmo = Mathf.RoundToInt(maxAmmo.currentValue);
                break;
            case "CooldownReduction":
                cooldownReduction.Add(value);
                if (gun != null)
                    gun.cooldownTime *= (1f - cooldownReduction.currentValue);
                break;
            default:
                Debug.LogWarning($"Tipo de efeito desconhecido: {effectType}");
                break;
        }
    }
}