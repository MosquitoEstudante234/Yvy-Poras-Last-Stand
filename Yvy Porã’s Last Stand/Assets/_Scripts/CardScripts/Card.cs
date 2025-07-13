using UnityEngine;

[System.Serializable]
public class Card
{
    public string cardName;
    [TextArea]
    public string description;
    public string translation;

    public Rarity rarity;
    public CardEffectType effectType;

    [Header("Efeito da carta (ao equipar)")]
    public float value;

    [Header("Penalidade ao queimar")]
    public BurnPenaltyType burnPenalty;
}

public enum Rarity
{
    Common,
    Rare,
    Legendary
}

public enum CardEffectType
{
    IncreaseDamage,
    IncreaseMaxHP,
    MovementSpeed,
    AmmoCapacity,
    ShieldPerWave,
    CooldownReduction,
    PoisonEffect,
    ReviveOnce,
    UpgradeCommonsToRare,
    PassiveRegenAndEvade,
    DamageAura,
    CardOnKill
}

public enum BurnPenaltyType
{
    ReduceMaxHP,
    ReduceDamage,
    LoseShield,
    SlowDown,
    CooldownPenalty,
    PoisonSelf,
    CurseNextCards,
    DisableRevive,
    None
}
