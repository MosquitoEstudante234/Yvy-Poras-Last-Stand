using UnityEngine;
using Photon.Pun;
using TMPro;

[RequireComponent(typeof(CharacterController), typeof(PhotonView))]
public class PlayerHealth : MonoBehaviourPun
{
    public int maxHealth = 100;
    private int currentHealth;

    private CharacterController controller;
    private Collider playerCollider;
    private Renderer[] renderers;

    public TextMeshProUGUI healthText; // Texto TMP da vida
    public GameObject deathCanvas;     // Canvas que avisa que o jogador morreu

    void Start()
    {
        currentHealth = maxHealth;
        controller = GetComponent<CharacterController>();
        playerCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();

        if (deathCanvas != null)
            deathCanvas.SetActive(false);

        UpdateHealthText();

        //coisa nova abaixo
        if (photonView.IsMine)
        {
            PhotonNetwork.LocalPlayer.TagObject = this;
        }
            
    }

    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthText();

        Debug.Log("Player levou dano. Vida atual: " + currentHealth);

        if (currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Vida: " + currentHealth.ToString();
        }
    }

    void HandleDeath()
    {
        Debug.Log("Player morreu.");

        // Desativa colisores e renderers
        if (playerCollider != null)
            playerCollider.enabled = false;

        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }

        if (controller != null)
            controller.detectCollisions = false;

        if (photonView.IsMine)
        {
            if (deathCanvas != null)
                deathCanvas.SetActive(true);
        }

        if (PhotonNetwork.IsConnected)
            GameManager.instance.PlayerDied();
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthText();
    }
}
