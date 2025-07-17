using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CardDraftManager : MonoBehaviourPun
{
    [Header("Referências")]
    public Button cardButton;         // Botão que o jogador clica para puxar uma carta
    public Image cardImage;           // Imagem que mostra o sprite da carta recebida
    public float cooldown = 90f;      // Tempo de recarga entre puxadas de carta

    [Header("Chances de Raridade (0 a 1)")]
    [Range(0f, 1f)] public float chanceCommon = 0.7f;
    [Range(0f, 1f)] public float chanceRare = 0.25f;
    [Range(0f, 1f)] public float chanceLegendary = 0.05f;

    private CardEffect currentCard;

    void Start()
    {
        if (photonView.IsMine)
        {
            cardButton.onClick.AddListener(OnClick_DrawCard);
            cardButton.interactable = true;
            cardImage.enabled = false; // Esconde imagem da carta até uma ser sorteada
        }
        else
        {
            // Esconde o botão para jogadores remotos
            cardButton.gameObject.SetActive(false);
        }
    }

    public void OnClick_DrawCard()
    {
        CardLibrary library = Object.FindFirstObjectByType<CardLibrary>();
        if (library == null) return;

        var rarity = RollRarity();
        CardEffect card = library.GetRandomCard(rarity);
        if (card == null) return;

        currentCard = card;

        // Exibe a imagem da carta
        cardImage.sprite = currentCard.sprite;
        cardImage.enabled = true;

        // Aplica o efeito ao jogador local
        ApplyCardEffectToLocalPlayer(currentCard);

        // Desativa botão e inicia cooldown
        cardButton.interactable = false;
        StartCoroutine(ReenableButtonAfterCooldown());
    }

    IEnumerator ReenableButtonAfterCooldown()
    {
        yield return new WaitForSeconds(cooldown);

        cardButton.interactable = true;
        cardImage.enabled = false;
        currentCard = null;
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

    void ApplyCardEffectToLocalPlayer(CardEffect card)
    {
        var player = FindLocalPlayer();
        if (player == null) return;

        var manager = player.GetComponent<CardManager>();
        if (manager != null)
            manager.ApplyCardEffect(card);
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
