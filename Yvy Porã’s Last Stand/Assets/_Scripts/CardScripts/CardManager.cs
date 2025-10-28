// 28/10/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardManager : MonoBehaviourPun
{
    [Header("Cartas do Jogador")]
    public List<Card> ownedCards = new List<Card>();

    // Adiciona uma carta ao inventário do jogador
    public void AddCardToInventory(Card card)
    {
        if (!photonView.IsMine) return;

        if (!ownedCards.Contains(card))
        {
            ownedCards.Add(card);
            Debug.Log($"Carta adicionada ao inventário: {card.cardName}");
        }
    }

    // Aplica os efeitos da carta ao jogador
    public void ApplyCardEffect(Card card)
    {
        if (!photonView.IsMine) return;

        if (!ownedCards.Contains(card))
        {
            AddCardToInventory(card);
        }

        photonView.RPC(nameof(ApplyCardEffectRPC), RpcTarget.All, card.effectType, card.value);
    }

    [PunRPC]
    private void ApplyCardEffectRPC(CardEffectType effectType, float value)
    {
        PlayerStats playerStats = GetComponent<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogError("Componente PlayerStats não encontrado no jogador.");
            return;
        }

        switch (effectType)
        {
            case CardEffectType.IncreaseDamage:
                playerStats.AddToStat("SpearDamage", value);
                break;

            case CardEffectType.IncreaseMaxHP:
                playerStats.AddToStat("MaxHealth", value);
                break;

            case CardEffectType.MovementSpeed:
                playerStats.AddToStat("MoveSpeed", value);
                break;

            case CardEffectType.AmmoCapacity:
                playerStats.AddToStat("MaxAmmo", value);
                break;

            case CardEffectType.ShieldPerWave:
                playerStats.AddToStat("Shield", value);
                break;

            case CardEffectType.CooldownReduction:
                playerStats.AddToStat("CooldownReduction", value);
                break;

            case CardEffectType.PoisonEffect:
                playerStats.AddToStat("PoisonEffect", value);
                break;

            case CardEffectType.ReviveOnce:
                playerStats.AddToStat("ReviveOnce", value);
                break;

            case CardEffectType.UpgradeCommonsToRare:
                // Implementação futura: Atualizar lógica para upgrade de cartas comuns para raras
                Debug.Log("Efeito de UpgradeCommonsToRare aplicado.");
                break;

            case CardEffectType.PassiveRegenAndEvade:
                playerStats.AddToStat("PassiveRegen", value);
                playerStats.AddToStat("Evasion", value);
                Debug.Log("Regeneração passiva e evasão ativadas.");
                break;

            default:
                Debug.LogWarning($"Tipo de efeito desconhecido: {effectType}");
                break;
        }

        Debug.Log($"Efeito da carta aplicado: {effectType} com valor {value}");
    }
}