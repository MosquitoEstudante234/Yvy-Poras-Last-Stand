// 28/10/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

// ScriptableObject para representar uma carta
[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card")]
public class Card : ScriptableObject
{
    [Header("Informações da Carta")]
    public string cardName;
    [TextArea]
    public string description;

    [Header("Atributos da Carta")]
    public Rarity rarity;
    public CardEffectType effectType;

    [Header("Efeito da carta (ao equipar)")]
    public float value;

    [Header("Penalidade ao queimar")]
    public BurnPenaltyType burnPenalty;

    public Sprite cardSprite;

    // Método para obter o sprite da carta
    public Sprite GetCardSprite()
    {
        return cardSprite;
    }
}

// Enum para representar a raridade das cartas
public enum Rarity
{
    Common,    // Cartas comuns
    Rare,      // Cartas raras
    Legendary  // Cartas lendárias
}

// Enum para representar os tipos de efeitos das cartas
public enum CardEffectType
{
    IncreaseDamage,           // Aumenta o dano
    IncreaseMaxHP,            // Aumenta a vida máxima
    MovementSpeed,            // Aumenta a velocidade de movimento
    AmmoCapacity,             // Aumenta a capacidade de munição
    ShieldPerWave,            // Gera escudo por onda
    CooldownReduction,        // Reduz o tempo de recarga
    PoisonEffect,             // Aplica efeito de veneno
    ReviveOnce,               // Revive uma vez
    UpgradeCommonsToRare,     // Melhora cartas comuns para raras
    PassiveRegenAndEvade,     // Regeneração passiva e esquiva
    DamageAura,               // Aura de dano
    CardOnKill                // Gera carta ao matar inimigos
}

// Enum para representar os tipos de penalidade ao queimar as cartas
public enum BurnPenaltyType
{
    ReduceMaxHP,              // Reduz a vida máxima
    ReduceDamage,             // Reduz o dano
    LoseShield,               // Perde o escudo
    SlowDown,                 // Diminui a velocidade
    CooldownPenalty,          // Penalidade no tempo de recarga
    PoisonSelf,               // Aplica veneno em si mesmo
    CurseNextCards,           // Amaldiçoa as próximas cartas
    DisableRevive,            // Desabilita o efeito de reviver
    None                      // Sem penalidade
}