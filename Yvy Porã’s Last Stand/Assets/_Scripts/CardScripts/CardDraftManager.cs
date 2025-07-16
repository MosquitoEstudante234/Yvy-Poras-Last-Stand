using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardDraftManager : MonoBehaviourPun
{
    [Header("Referências")]
    public GameObject cardUIPrefab;
    public Transform cardUIParent;

    [Header("Chances de Raridade (0 a 1)")]
    [Range(0f, 1f)] public float chanceCommon = 0.7f;
    [Range(0f, 1f)] public float chanceRare = 0.25f;
    [Range(0f, 1f)] public float chanceLegendary = 0.05f;

    [Header("Tempo entre novas cartas (segundos)")]
    public float intervalSeconds = 90f;

    private List<CardEffect> pendingCards = new();

    void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(TimedCardGain());
        }
    }

    IEnumerator TimedCardGain()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalSeconds);
            AddCardsToPending(3);
        }
    }

    void AddCardsToPending(int amount)
    {
        CardLibrary library = Object.FindFirstObjectByType<CardLibrary>();
        if (library == null) return;

        for (int i = 0; i < amount; i++)
        {
            var rarity = RollRarity();
            CardEffect card = library.GetRandomCard(rarity);
            if (card != null)
                pendingCards.Add(card);
        }

        Debug.Log("Cartas disponíveis para resgate no NPC: " + pendingCards.Count);
    }

    CardEffect.CardType RollRarity()
    {
        float roll = Random.value;

        if (roll < chanceLegendary)
            return CardEffect.CardType.Legendary;
        else if (roll < chanceLegendary + chanceRare)
            return CardEffect.CardType.Rare;
        else
            return CardEffect.CardType.Common;
    }

    public void ShowPendingCards()
    {
        foreach (Transform child in cardUIParent)
            Destroy(child.gameObject);

        foreach (var card in pendingCards)
        {
            GameObject ui = Instantiate(cardUIPrefab, cardUIParent);
            CardUI uiScript = ui.GetComponent<CardUI>();
            uiScript.SetCard(card);
        }

        pendingCards.Clear();
    }
}
