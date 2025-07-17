using Photon.Pun;
using StarterAssets;
using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviourPun
{
    public List<CardEffect> ownedCards = new();

    public void ApplyCardEffect(CardEffect card)
    {
        if (!photonView.IsMine) return;

        if (!ownedCards.Contains(card))
            ownedCards.Add(card);

        photonView.RPC(nameof(ApplyCardEffectRPC), photonView.Owner, (int)card.stat, card.value);
    }

    [PunRPC]
    void ApplyCardEffectRPC(int statIndex, float value)
    {
        var stat = (CardEffect.StatType)statIndex;
        GameObject player = gameObject;

        switch (stat)
        {
            case CardEffect.StatType.MoveSpeed:
                player.GetComponent<FirstPersonController>().MoveSpeed += value;
                break;
            case CardEffect.StatType.SprintSpeed:
                player.GetComponent<FirstPersonController>().SprintSpeed += value;
                break;
            case CardEffect.StatType.SpearDamage:
                player.GetComponentInChildren<Spear>().damage += Mathf.RoundToInt(value);
                break;
            case CardEffect.StatType.MaxHealth:
                player.GetComponent<PlayerHealth>().maxHealth += Mathf.RoundToInt(value);
                break;
            case CardEffect.StatType.MaxAmmo:
                player.GetComponentInChildren<Gun>().maxAmmo += Mathf.RoundToInt(value);
                break;
            case CardEffect.StatType.CooldownReduction:
                player.GetComponentInChildren<Gun>().cooldownTime *= 1f - value;
                break;
            case CardEffect.StatType.Shield:
                Debug.Log("Escudo aplicado: implementar lógica de escudo regenerável.");
                break;
            case CardEffect.StatType.ConvertCommonsToRares:
                Debug.Log("Todas cartas comuns se tornarão raras.");
                break;
            case CardEffect.StatType.PassiveRegen:
                Debug.Log("Regeneração passiva ativada.");
                break;
            case CardEffect.StatType.PoisonEnemies:
                Debug.Log("Ataques agora envenenam inimigos.");
                break;
            case CardEffect.StatType.AoEDamageAura:
                Debug.Log("Aura de dano contínuo ativada.");
                break;
            case CardEffect.StatType.ReviveOnDeath:
                Debug.Log("O jogador será revivido automaticamente na morte.");
                break;
            case CardEffect.StatType.DrawCardOnKill:
                Debug.Log("O jogador ganha carta a cada 5 inimigos mortos.");
                break;
        }

        Debug.Log($"Carta aplicada: {stat} + {value}");
    }

    public void BurnCard(CardEffect card)
    {
        if (!photonView.IsMine) return;

        if (ownedCards.Contains(card))
            ownedCards.Remove(card);

        photonView.RPC(nameof(ApplyBurnEffectRPC), photonView.Owner, (int)card.burnEffect, card.burnValue);
    }

    [PunRPC]
    void ApplyBurnEffectRPC(int effectIndex, float value)
    {
        var effect = (CardEffect.BurnEffectType)effectIndex;

        switch (effect)
        {
            case CardEffect.BurnEffectType.LoseAmmo:
                GetComponentInChildren<Gun>().maxAmmo -= Mathf.RoundToInt(value);
                break;
            case CardEffect.BurnEffectType.LoseHealth:
                GetComponent<PlayerHealth>().maxHealth -= Mathf.RoundToInt(value);
                break;
            case CardEffect.BurnEffectType.IncreaseCooldown:
                GetComponentInChildren<Gun>().cooldownTime *= 1f + value;
                break;
        }

        Debug.Log($"Carta queimada: {effect} -{value}");
    }
}
