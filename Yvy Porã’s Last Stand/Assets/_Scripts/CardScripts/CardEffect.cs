using StarterAssets;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card Effect")]
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

    public void ApplyEffect(GameObject player)
    {
        switch (stat)
        {
            case StatType.MoveSpeed:
                player.GetComponent<FirstPersonController>().MoveSpeed += value;
                break;
            case StatType.SprintSpeed:
                player.GetComponent<FirstPersonController>().SprintSpeed += value;
                break;
            case StatType.SpearDamage:
                player.GetComponentInChildren<Spear>().damage += Mathf.RoundToInt(value);
                break;
            case StatType.MaxHealth:
                player.GetComponent<PlayerHealth>().maxHealth += Mathf.RoundToInt(value);
                break;
            case StatType.MaxAmmo:
                player.GetComponentInChildren<Gun>().maxAmmo += Mathf.RoundToInt(value);
                break;
            case StatType.CooldownReduction:
                player.GetComponentInChildren<Gun>().cooldownTime *= 1f - value;
                break;
            case StatType.Shield:
                Debug.Log("Escudo aplicado. Requer implementação de sistema de escudo.");
                break;
            case StatType.PoisonEnemies:
                Debug.Log("Efeito de veneno aplicado. Requer lógica em ataques.");
                break;
            case StatType.PassiveRegen:
                Debug.Log("Regeneração passiva ativada.");
                break;
            case StatType.AoEDamageAura:
                Debug.Log("Aura de dano contínuo ativada.");
                break;
            case StatType.ReviveOnDeath:
                Debug.Log("O jogador será revivido automaticamente na morte.");
                break;
            case StatType.ConvertCommonsToRares:
                Debug.Log("Cartas comuns futuras serão convertidas em raras.");
                break;
            case StatType.DrawCardOnKill:
                Debug.Log("Jogador ganhará carta a cada 5 inimigos mortos.");
                break;
        }
    }
}
