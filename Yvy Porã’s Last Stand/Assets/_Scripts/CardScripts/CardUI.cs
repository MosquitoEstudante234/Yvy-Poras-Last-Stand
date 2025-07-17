using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Image cardImage;
    private CardEffect cardEffect;

    [Header("Referência via Inspector")]
    public CardDraftManager cardDraftManager;

    public void SetCard(CardEffect effect)
    {
        cardEffect = effect;
        cardImage.sprite = effect.sprite;
    }

    public void OnClickApplyCard()
    {
        var player = FindLocalPlayer();
        if (player != null)
        {
            var manager = player.GetComponent<CardManager>();
            manager.ApplyCardEffect(cardEffect);
        }
    }

    public void OnClick_ShowCards()
    {
        if (cardDraftManager != null)
            cardDraftManager.ShowPendingCards();
    }

    GameObject FindLocalPlayer()
    {
        foreach (var p in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView view = p.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
                return p;
        }
        return null;
    }
}
