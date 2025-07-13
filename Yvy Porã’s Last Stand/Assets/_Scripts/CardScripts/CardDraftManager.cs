using System.Collections;
using UnityEngine;
using Photon.Pun;

public class CardDraftManager : MonoBehaviourPun
{
    public GameObject cardUIPrefab;
    public Transform cardUIParent;
    public float showDelay = 2f;

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

        if (wave % 5 == 0)
        {
            StartCoroutine(ShowCardDraftAfterDelay());
        }
    }

    IEnumerator ShowCardDraftAfterDelay()
    {
        yield return new WaitForSeconds(showDelay);

        CardLibrary library = Object.FindFirstObjectByType<CardLibrary>();
        var draft = library.GetRandomCards(3, CardEffect.CardType.Common); // ou outro tipo

        foreach (CardEffect card in draft)
        {
            GameObject ui = Instantiate(cardUIPrefab, cardUIParent);
            CardUI uiScript = ui.GetComponent<CardUI>();
            uiScript.SetCard(card);
        }
    }
}
