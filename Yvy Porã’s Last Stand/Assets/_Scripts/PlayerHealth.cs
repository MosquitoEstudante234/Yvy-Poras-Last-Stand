using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;

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

    [Header("Passive Regen Settings")]
    public bool enablePassiveRegen = false; // Toggle for passive regeneration
    public float regenRate = 1f; // Health regenerated per second
    public float regenInterval = 1f; // Time interval for regeneration

    private void Start()
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

            if (enablePassiveRegen)
            {
                StartCoroutine(PassiveRegeneration());
            }
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

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Vida: " + currentHealth.ToString();
        }
    }

    // Passive regeneration coroutine
    private IEnumerator PassiveRegeneration()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(regenInterval);

            if (currentHealth < maxHealth)
            {
                currentHealth += Mathf.RoundToInt(regenRate);
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                UpdateHealthText();
            }
        }
    }

    [PunRPC]
    private void RPC_SetDead()
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

        // Only the MasterClient counts deaths
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
        {
            GameManager.instance.PlayerDied();
        }
    }

    [PunRPC]
    private void RPC_NotifyDeathToMaster()
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

    public void EnablePassiveRegen(bool enable)
    {
        enablePassiveRegen = enable;

        if (enable && photonView.IsMine)
        {
            StartCoroutine(PassiveRegeneration());
        }
        else
        {
            StopCoroutine(PassiveRegeneration());
        }
    }
}