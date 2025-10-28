// 28/10/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

[CreateAssetMenu(fileName = "NewCardEffect", menuName = "Cards/Card Effect")]
public class CardEffect : ScriptableObject
{
    public enum StatType
    {
        MoveSpeed,
        SprintSpeed,
        MaxHealth,
        MaxAmmo,
        SpearDamage,
        CooldownReduction,
        Shield,
        PoisonEnemies,
        PassiveRegen,
        AoEDamageAura,
        ReviveOnDeath,
        ConvertCommonsToRares,
        DrawCardOnKill
    }

    public enum CardType
    {
        Common,
        Rare,
        Legendary
    }

    public enum BurnEffectType
    {
        None,
        LoseAmmo,
        LoseHealth,
        IncreaseCooldown
    }

    public string cardName;
    [TextArea] public string description;
    public Sprite sprite;

    public StatType stat;
    public CardType type;
    public float value;

    public BurnEffectType burnEffect;
    public float burnValue;

    public void ApplyEffect(PlayerStats playerStats)
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component não encontrado no jogador.");
            return;
        }

        switch (stat)
        {
            case StatType.MoveSpeed:
                playerStats.AddToStat("MoveSpeed", value);
                break;
            case StatType.SprintSpeed:
                playerStats.AddToStat("SprintSpeed", value);
                break;
            case StatType.SpearDamage:
                playerStats.AddToStat("SpearDamage", value);
                break;
            case StatType.MaxHealth:
                playerStats.AddToStat("MaxHealth", Mathf.RoundToInt(value));
                break;
            case StatType.MaxAmmo:
                playerStats.AddToStat("MaxAmmo", Mathf.RoundToInt(value));
                break;
            case StatType.CooldownReduction:
                playerStats.AddToStat("CooldownReduction", value);
                break;
            case StatType.Shield:
                playerStats.AddToStat("Shield", value);
                break;
            case StatType.PoisonEnemies:
                playerStats.AddToStat("PoisonEffect", value);
                break;
            case StatType.PassiveRegen:
                playerStats.AddToStat("PassiveRegen", value);
                break;
            case StatType.AoEDamageAura:
                playerStats.AddToStat("AoEDamageAura", value);
                break;
            case StatType.ReviveOnDeath:
                playerStats.AddToStat("ReviveOnce", value);
                break;
            case StatType.ConvertCommonsToRares:
                playerStats.AddToStat("UpgradeCommonsToRare", value);
                break;
            case StatType.DrawCardOnKill:
                playerStats.AddToStat("CardOnKill", value);
                break;
            default:
                Debug.LogWarning($"Unknown stat type: {stat}");
                break;
        }
    }
}