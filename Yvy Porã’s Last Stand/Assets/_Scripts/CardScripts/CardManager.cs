using Photon.Pun;
using StarterAssets;
using UnityEngine;

public class CardManager : MonoBehaviourPun
{
    public void ApplyCardEffect(CardEffect card)
    {
        if (!photonView.IsMine) return;

        photonView.RPC(nameof(ApplyCardEffectRPC), RpcTarget.AllBuffered, (int)card.stat, card.value);
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
                // Implementar sistema de escudo no PlayerHealth futuramente
                Debug.Log("Escudo aplicado: implementar l�gica de escudo regener�vel.");
                break;

            case CardEffect.StatType.ConvertCommonsToRares:
                // Marcar flag em um gerenciador de cartas local
                Debug.Log("Todas cartas comuns se tornar�o raras.");
                break;

            case CardEffect.StatType.PassiveRegen:
                // Exemplo: ativar regenera��o passiva (precisa ser implementada)
                Debug.Log("Regenera��o passiva ativada.");
                break;

            case CardEffect.StatType.PoisonEnemies:
                // Ativar l�gica de veneno em ataques futuros
                Debug.Log("Ataques agora envenenam inimigos.");
                break;

            case CardEffect.StatType.AoEDamageAura:
                // Ativar dano de �rea passivo
                Debug.Log("Aura de dano cont�nuo ativada.");
                break;

            case CardEffect.StatType.ReviveOnDeath:
                // Ativar l�gica de auto-revive
                Debug.Log("O jogador ser� revivido automaticamente na morte.");
                break;

            case CardEffect.StatType.DrawCardOnKill:
                // Exemplo: implementar l�gica de drop de carta por kill
                Debug.Log("O jogador ganha carta a cada 5 inimigos mortos.");
                break;
        }

        Debug.Log($"Carta aplicada: {stat} + {value}");
    }
}
