// 28/10/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CardDraftManager : MonoBehaviourPunCallbacks
{
    [Header("Referências")]
    public Button cardButton; // Botão que o jogador clica para puxar uma carta
    public Image cardImage; // Imagem que mostra o sprite da carta recebida
    public float cooldown = 90f; // Tempo de recarga entre puxadas de carta

    [Header("Chances de Raridade (0 a 1)")]
    [Range(0f, 1f)] public float chanceCommon = 0.7f;
    [Range(0f, 1f)] public float chanceRare = 0.25f;
    [Range(0f, 1f)] public float chanceLegendary = 0.05f;

    private Card currentCard;
    private CardLibrary cardLibrary; // Referência ao CardLibrary para economizar processamento

    private void Awake()
    {
        cardLibrary = FindObjectOfType<CardLibrary>();

        if (cardLibrary == null)
        {
            Debug.LogError("CardLibrary não encontrado na cena.");
        }
    }

    private void Start()
    {
        SetupPlayer();
    }

    private void SetupPlayer()
    {
        if (photonView.IsMine)
        {
            InitializeCardButton();
            HideCardImage();
        }
        else
        {
            HideCardButton();
        }
    }

    private void InitializeCardButton()
    {
        cardButton.onClick.AddListener(OnClick_DrawCard);
        cardButton.interactable = true;
    }

    private void HideCardImage()
    {
        cardImage.enabled = false; // Esconde imagem da carta até uma ser sorteada
    }

    private void HideCardButton()
    {
        cardButton.gameObject.SetActive(false);
    }

    public void OnClick_DrawCard()
    {
        if (cardButton.interactable)
        {
            StartCoroutine(DrawCardWithCooldown());
        }
    }

    private IEnumerator DrawCardWithCooldown()
    {
        cardButton.interactable = false;
        DrawCard();
        yield return new WaitForSeconds(cooldown);
        cardButton.interactable = true;
        HideCardImage();
        currentCard = null;
    }

    private void DrawCard()
    {
        if (cardLibrary == null)
        {
            Debug.LogError("CardLibrary não encontrado. Certifique-se de que está na cena.");
            return;
        }

        Rarity rarity = RollRarity();
        Card card = cardLibrary.GetRandomCard(rarity);

        if (card == null)
        {
            Debug.LogWarning($"Nenhuma carta encontrada para a raridade: {rarity}");
            return;
        }

        currentCard = card;
        DisplayCardImage(card);
        AddCardToInventory(card);
        ApplyCardEffectToPlayer(card);
    }

    private Rarity RollRarity()
    {
        float roll = Random.value;
        if (roll < chanceLegendary)
            return Rarity.Legendary;
        else if (roll < chanceLegendary + chanceRare)
            return Rarity.Rare;
        else
            return Rarity.Common;
    }

    private void DisplayCardImage(Card card)
    {
        if (card == null)
        {
            Debug.LogWarning("Tentativa de exibir uma carta nula.");
            return;
        }

        cardImage.sprite = card.GetCardSprite();
        cardImage.enabled = true;
    }

    private void AddCardToInventory(Card card)
    {
        var localPlayer = FindLocalPlayer();
        if (localPlayer == null)
        {
            Debug.LogError("Nenhum jogador local encontrado.");
            return;
        }

        var cardManager = localPlayer.GetComponent<CardManager>();
        if (cardManager != null)
        {
            cardManager.AddCardToInventory(card);
        }
        else
        {
            Debug.LogError("CardManager não encontrado no jogador local.");
        }
    }

    private void ApplyCardEffectToPlayer(Card card)
    {
        var localPlayer = FindLocalPlayer();
        if (localPlayer == null)
        {
            Debug.LogError("Nenhum jogador local encontrado.");
            return;
        }

        var playerStats = localPlayer.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.ApplyCardEffect(card.effectType.ToString(), card.value); // Centralização na lógica de PlayerStats
        }
        else
        {
            Debug.LogError("PlayerStats não encontrado no jogador local.");
        }
    }

    private GameObject FindLocalPlayer()
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView view = player.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
            {
                return player;
            }
        }

        return null;
    }
}