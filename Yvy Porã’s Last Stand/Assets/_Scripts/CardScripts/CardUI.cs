using Photon.Pun;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText;
    public Image cardImage;

    private CardEffect cardEffect;

    public void SetCard(CardEffect effect)
    {
        cardEffect = effect;

        cardNameText.text = effect.cardName;
        cardDescriptionText.text = effect.description;
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
