using UnityEngine;
using Photon.Pun;
using TMPro;

[RequireComponent(typeof(CharacterController), typeof(PhotonView))]
public class PlayerHealth : MonoBehaviourPun
{
    public int maxHealth = 100;
    private int currentHealth;
    public bool isDead { get; private set; } = false;

    private CharacterController controller;
    private Collider playerCollider;
    private Renderer[] renderers;

    public TextMeshProUGUI healthText;
    public GameObject deathCanvas;

    void Start()
    {
        currentHealth = maxHealth;
        controller = GetComponent<CharacterController>();
        playerCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();

        if (deathCanvas != null)
            deathCanvas.SetActive(false);

        UpdateHealthText();

        if (photonView.IsMine)
        {
            PhotonNetwork.LocalPlayer.TagObject = this;
        }
    }

    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthText();

        if (currentHealth <= 0)
        {
            photonView.RPC(nameof(RPC_SetDead), RpcTarget.All);
            photonView.RPC(nameof(RPC_NotifyDeathToMaster), RpcTarget.MasterClient);
        }
    }

    void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Vida: " + currentHealth.ToString();
        }
    }

    [PunRPC]
    void RPC_SetDead()
    {
        if (isDead) return;

        isDead = true;

        if (playerCollider != null)
            playerCollider.enabled = false;

        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }

        if (controller != null)
            controller.detectCollisions = false;

        if (photonView.IsMine && deathCanvas != null)
            deathCanvas.SetActive(true);

        // Corrigido: só o MasterClient conta mortes
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
        {
            GameManager.instance.PlayerDied();
        }
    }
    [PunRPC]
    void RPC_NotifyDeathToMaster()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
        {
            GameManager.instance.PlayerDied();
        }
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthText();
    }
}
