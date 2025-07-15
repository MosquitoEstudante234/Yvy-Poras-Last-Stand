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

    public float showDelay = 2f;

    private List<CardEffect> pendingCards = new();

    private void OnEnable()
    {
        WaveSpawner.OnWaveCompleted += HandleWaveCompleted;
    }

    private void OnDisable()
    {
        WaveSpawner.OnWaveCompleted -= HandleWaveCompleted;
    }

    private void HandleWaveCompleted(int wave)
    {
        if (!photonView.IsMine) return;

        if (wave == 6)
        {
            StartCoroutine(ShowCardDraftAfterDelay());
        }
    }

    IEnumerator ShowCardDraftAfterDelay()
    {
        yield return new WaitForSeconds(showDelay);

        CardLibrary library = Object.FindFirstObjectByType<CardLibrary>();
        if (library == null) yield break;

        pendingCards.Clear();

        for (int i = 0; i < 3; i++)
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
        // Limpa cartas antigas na UI
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
