using Photon.Pun;
using StarterAssets;
using UnityEngine;

public class CardManager : MonoBehaviourPun
{
    public void ApplyCardEffect(CardEffect card)
    {
        if (!photonView.IsMine) return;

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
                var controller = player.GetComponent<FirstPersonController>();
                controller.MoveSpeed += value;
                break;

            case CardEffect.StatType.SprintSpeed:
                player.GetComponent<FirstPersonController>().SprintSpeed += value;
                break;

            case CardEffect.StatType.SpearDamage:
                player.GetComponentInChildren<Spear>().damage += Mathf.RoundToInt(value);
                break;

            case CardEffect.StatType.MaxHealth:
                var health = player.GetComponent<PlayerHealth>();
                health.maxHealth += Mathf.RoundToInt(value);
                break;

            case CardEffect.StatType.MaxAmmo:
                player.GetComponentInChildren<Gun>().maxAmmo += Mathf.RoundToInt(value);
                break;

            case CardEffect.StatType.CooldownReduction:
                player.GetComponentInChildren<Gun>().cooldownTime *= 1f - value;
                break;

            case CardEffect.StatType.Shield:
                Debug.Log("Escudo aplicado: implementar l�gica de escudo regener�vel.");
                break;

            case CardEffect.StatType.ConvertCommonsToRares:
                Debug.Log("Todas cartas comuns se tornar�o raras.");
                break;

            case CardEffect.StatType.PassiveRegen:
                Debug.Log("Regenera��o passiva ativada.");
                break;

            case CardEffect.StatType.PoisonEnemies:
                Debug.Log("Ataques agora envenenam inimigos.");
                break;

            case CardEffect.StatType.AoEDamageAura:
                Debug.Log("Aura de dano cont�nuo ativada.");
                break;

            case CardEffect.StatType.ReviveOnDeath:
                Debug.Log("O jogador ser� revivido automaticamente na morte.");
                break;

            case CardEffect.StatType.DrawCardOnKill:
                Debug.Log("O jogador ganha carta a cada 5 inimigos mortos.");
                break;
        }

        Debug.Log($"Carta aplicada: {stat} + {value}");
    }
}
