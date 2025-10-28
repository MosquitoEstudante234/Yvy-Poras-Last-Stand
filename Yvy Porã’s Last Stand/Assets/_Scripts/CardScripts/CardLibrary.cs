using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardLibrary", menuName = "Cards/Card Library")]
public class CardLibrary : ScriptableObject
{
    public List<Card> commonCards;
    public List<Card> rareCards;
    public List<Card> legendaryCards;

    public Card GetRandomCard(Rarity rarity)
    {
        List<Card> pool = rarity switch
        {
            Rarity.Common => commonCards,
            Rarity.Rare => rareCards,
            Rarity.Legendary => legendaryCards,
            _ => null
        };

        if (pool == null || pool.Count == 0) return null;

        return pool[Random.Range(0, pool.Count)];
    }
}