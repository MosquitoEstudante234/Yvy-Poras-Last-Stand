using Photon.Pun;
using UnityEngine;

public class CardUI : MonoBehaviour
{
    public CardEffect cardEffect;

    public void OnClickApplyCard()
    {
        // Acha o CardManager do próprio player local
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
