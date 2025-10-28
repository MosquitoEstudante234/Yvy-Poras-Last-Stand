using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardManager : MonoBehaviourPun
{
    public List<Card> ownedCards = new List<Card>();

    // Add card to player's inventory
    public void AddCardToInventory(Card card)
    {
        if (!photonView.IsMine) return;

        if (!ownedCards.Contains(card))
        {
            ownedCards.Add(card);
            Debug.Log($"Card added to inventory: {card.cardName}");
        }
    }

    // Apply card effects to the player
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
        GameObject player = photonView.gameObject; // Reference to the player object (local to this instance)

        PlayerStats playerStats = player.GetComponent<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on player.");
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
                playerStats.AddToStat("ShieldPerWave", value);
                Debug.Log("Shield per wave effect applied.");
                break;

            case CardEffectType.CooldownReduction:
                playerStats.AddToStat("CooldownReduction", value);
                break;

            case CardEffectType.PoisonEffect:
                playerStats.AddToStat("PoisonEffect", value);
                Debug.Log("Poison effect applied.");
                break;

            case CardEffectType.ReviveOnce:
                playerStats.AddToStat("ReviveOnce", value);
                Debug.Log("Player will be revived automatically upon death.");
                break;

            case CardEffectType.UpgradeCommonsToRare:
                playerStats.AddToStat("UpgradeCommonsToRare", value);
                Debug.Log("Common cards upgraded to rare.");
                break;

            case CardEffectType.PassiveRegenAndEvade:
                playerStats.AddToStat("PassiveRegen", value);
                playerStats.AddToStat("Evasion", value);
                Debug.Log("Passive regeneration and evasion activated.");
                break;

            case CardEffectType.DamageAura:
                playerStats.AddToStat("DamageAura", value);
                Debug.Log("Damage aura activated.");
                break;

            case CardEffectType.CardOnKill:
                playerStats.AddToStat("CardOnKill", value);
                Debug.Log("Card will be received upon killing an enemy.");
                break;

            default:
                Debug.LogWarning($"Unknown effect type: {effectType}");
                break;
        }

        Debug.Log($"Card effect applied: {effectType} with value {value}");
    }
}