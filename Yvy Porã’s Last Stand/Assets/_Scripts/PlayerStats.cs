using UnityEngine;
using Photon.Pun;
using StarterAssets;

[System.Serializable]
public class Stat
{
    public string name;
    public float baseValue;
    public float currentValue;

    public void Reset() => currentValue = baseValue;
    public void Add(float amount) => currentValue += amount;
    public void Multiply(float multiplier) => currentValue *= multiplier;
}

public class PlayerStats : MonoBehaviourPun
{
    public Stat maxHealth;
    public Stat moveSpeed;
    public Stat sprintSpeed;
    public Stat spearDamage;
    public Stat maxAmmo;
    public Stat cooldownReduction;

    private PlayerHealth health;
    private FirstPersonController controller;
    private Gun gun;
    private Spear spear;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        controller = GetComponent<FirstPersonController>();
        gun = GetComponentInChildren<Gun>();
        spear = GetComponentInChildren<Spear>();

        ResetAllStats(); // Inicializa os valores
        ApplyStats();
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
            case "MaxHealth": maxHealth.Add(value); break;
            case "MoveSpeed": moveSpeed.Add(value); break;
            case "SprintSpeed": sprintSpeed.Add(value); break;
            case "SpearDamage": spearDamage.Add(value); break;
            case "MaxAmmo": maxAmmo.Add(value); break;
            case "CooldownReduction": cooldownReduction.Add(value); break;
        }

        ApplyStats();
    }
}
