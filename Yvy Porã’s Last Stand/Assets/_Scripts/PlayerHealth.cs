using UnityEngine;
using Photon.Pun;
using TMPro;

[RequireComponent(typeof(CharacterController), typeof(PhotonView))]
public class PlayerHealth : MonoBehaviourPun
{
    public int maxHealth = 100;
    private int currentHealth;

    public float spectatorSpeed = 5f;
    private bool isSpectator = false;

    private CharacterController controller;
    private Collider playerCollider;
    private Renderer[] renderers;

    public TextMeshProUGUI healthText; // Texto TMP da vida

    void Start()
    {
        currentHealth = maxHealth;
        controller = GetComponent<CharacterController>();
        playerCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();

        UpdateHealthText(); // Mostra a vida inicial
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (isSpectator)
        {
            SpectatorMovement();
        }
    }

    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine || isSpectator) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthText();

        Debug.Log("Player levou dano. Vida atual: " + currentHealth);

        if (currentHealth <= 0)
        {
            EnterSpectatorMode();
        }
    }

    void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Vida: " + currentHealth.ToString();
        }
    }

    void EnterSpectatorMode()
    {
        Debug.Log("Entrando no modo espectador...");
        isSpectator = true;

        if (playerCollider != null)
            playerCollider.enabled = false;

        foreach (Renderer rend in renderers)
        {
            if (rend.material.HasProperty("_Color"))
            {
                Color color = rend.material.color;
                color.a = 0.5f;
                rend.material.color = color;
            }
        }

        if (controller != null)
            controller.detectCollisions = false;

        if (PhotonNetwork.IsConnected)
            GameManager.instance.PlayerDied();
    }

    void SpectatorMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float upDown = 0f;

        if (Input.GetKey(KeyCode.Space)) upDown = 1f;
        else if (Input.GetKey(KeyCode.LeftShift)) upDown = -1f;

        Vector3 move = new Vector3(horizontal, upDown, vertical);
        controller.Move(transform.TransformDirection(move) * spectatorSpeed * Time.deltaTime);
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthText();
    }
}
