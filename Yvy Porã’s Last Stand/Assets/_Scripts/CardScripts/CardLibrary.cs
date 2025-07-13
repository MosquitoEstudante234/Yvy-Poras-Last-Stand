using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardLibrary", menuName = "Cards/Card Library")]
public class CardLibrary : ScriptableObject
{
    public List<CardEffect> commonCards;
    public List<CardEffect> rareCards;
    public List<CardEffect> legendaryCards;

    [Range(0f, 1f)] public float rareChance = 0.25f;
    [Range(0f, 1f)] public float legendaryChance = 0.05f;

    public List<CardEffect> GetRandomCards(int count)
    {
        List<CardEffect> result = new();

        while (result.Count < count)
        {
            CardEffect.CardType type = GetRandomCardType();
            CardEffect card = GetRandomCard(type);

            if (card != null && !result.Contains(card))
                result.Add(card);
        }

        return result;
    }

    public List<CardEffect> GetRandomCards(int count, CardEffect.CardType type)
    {
        List<CardEffect> pool = type switch
        {
            CardEffect.CardType.Common => commonCards,
            CardEffect.CardType.Rare => rareCards,
            CardEffect.CardType.Legendary => legendaryCards,
            _ => null
        };

        List<CardEffect> result = new();
        if (pool == null || pool.Count == 0) return result;

        List<CardEffect> tempPool = new(pool);
        for (int i = 0; i < count && tempPool.Count > 0; i++)
        {
            int index = Random.Range(0, tempPool.Count);
            result.Add(tempPool[index]);
            tempPool.RemoveAt(index);
        }

        return result;
    }

    public CardEffect GetRandomCard(CardEffect.CardType type)
    {
        List<CardEffect> pool = type switch
        {
            CardEffect.CardType.Common => commonCards,
            CardEffect.CardType.Rare => rareCards,
            CardEffect.CardType.Legendary => legendaryCards,
            _ => null
        };

        if (pool == null || pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    private CardEffect.CardType GetRandomCardType()
    {
        float roll = Random.value;

        if (roll < legendaryChance)
            return CardEffect.CardType.Legendary;
        else if (roll < legendaryChance + rareChance)
            return CardEffect.CardType.Rare;
        else
            return CardEffect.CardType.Common;
    }
}
