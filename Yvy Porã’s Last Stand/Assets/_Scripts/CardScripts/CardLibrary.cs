// 28/10/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardLibrary", menuName = "Cards/Card Library")]
public class CardLibrary : ScriptableObject
{
    [Header("Listas de Cartas por Raridade")]
    public List<Card> commonCards;
    public List<Card> rareCards;
    public List<Card> legendaryCards;

    // Retorna uma carta aleatória com base na raridade
    public Card GetRandomCard(Rarity rarity)
    {
        List<Card> pool;

        switch (rarity)
        {
            case Rarity.Common:
                pool = commonCards;
                break;
            case Rarity.Rare:
                pool = rareCards;
                break;
            case Rarity.Legendary:
                pool = legendaryCards;
                break;
            default:
                Debug.LogError($"Raridade inválida: {rarity}");
                return null;
        }

        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning($"Lista de cartas está vazia para a raridade: {rarity}");
            return null;
        }

        return pool[Random.Range(0, pool.Count)];
    }
}