using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
using MOBAGame.Core; // NOVO: Para acessar o enum Team

namespace MOBAGame.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PhotonView))]
    public class PlayerHealth : MonoBehaviourPun
    {
        [Header("Health Settings")]
        public int maxHealth = 100;
        private int currentHealth;
        public bool isDead { get; private set; } = false;

        [Header("Components")]
        private CharacterController controller;
        private Collider playerCollider;
        private Renderer[] renderers;

        [Header("UI References")]
        public TextMeshProUGUI healthText;
        public GameObject deathCanvas;
        public TextMeshProUGUI respawnTimerText; // NOVO: Para mostrar countdown

        [Header("Passive Regen Settings")]
        public bool enablePassiveRegen = false;
        public float regenRate = 1f;
        public float regenInterval = 1f;

        [Header("Respawn Settings")]
        public float respawnTime = 7f; // NOVO: Tempo de respawn (7 segundos)
        private float respawnTimer = 0f;

        // NOVO: Referência ao time do jogador
        private Team playerTeam = Team.None;

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

                // NOVO: Obtém o time do jogador das propriedades customizadas
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
                {
                    playerTeam = (Team)((int)teamValue);
                }

                if (enablePassiveRegen)
                {
                    StartCoroutine(PassiveRegeneration());
                }
            }
        }

        /// <summary>
        /// MODIFICADO: Agora recebe quem causou o dano para validar PvP
        /// </summary>
        public void TakeDamage(int damage, PhotonView attacker = null)
        {
            if (!photonView.IsMine || isDead) return;

            // NOVO: Validação de friendly fire (jogadores do mesmo time não podem se atacar)
            if (attacker != null && attacker.Owner != null)
            {
                if (attacker.Owner.CustomProperties.TryGetValue("Team", out object attackerTeamValue))
                {
                    Team attackerTeam = (Team)((int)attackerTeamValue);
                    if (attackerTeam == playerTeam && attackerTeam != Team.None)
                    {
                        // Mesmo time, ignora dano
                        return;
                    }
                }
            }

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateHealthText();

            if (currentHealth <= 0)
            {
                photonView.RPC(nameof(RPC_SetDead), RpcTarget.All);

                // NOVO: Inicia sistema de respawn
                if (photonView.IsMine)
                {
                    StartCoroutine(RespawnCountdown());
                }
            }
        }

        private void UpdateHealthText()
        {
            if (healthText != null)
            {
                healthText.text = "Vida: " + currentHealth.ToString();
            }
        }

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

        // NOVO: Sistema de countdown para respawn
        private IEnumerator RespawnCountdown()
        {
            respawnTimer = respawnTime;

            while (respawnTimer > 0)
            {
                if (respawnTimerText != null)
                {
                    respawnTimerText.text = $"Respawn em: {Mathf.CeilToInt(respawnTimer)}s";
                }

                respawnTimer -= Time.deltaTime;
                yield return null;
            }

            // Após 7 segundos, respawna na base do time
            Respawn();
        }

        // NOVO: Sistema de respawn
        private void Respawn()
        {
            if (!photonView.IsMine) return;

            // Reseta vida
            currentHealth = maxHealth;
            UpdateHealthText();

            // Reativa componentes
            photonView.RPC(nameof(RPC_Respawn), RpcTarget.All);

            // NOVO: Teleporta para spawn point do time
            Transform spawnPoint = GameManager.instance.GetSpawnPointForTeam(playerTeam);
            if (spawnPoint != null)
            {
                controller.enabled = false;
                transform.position = spawnPoint.position;
                transform.rotation = spawnPoint.rotation;
                controller.enabled = true;
            }

            if (deathCanvas != null)
                deathCanvas.SetActive(false);

            // Reinicia regeneração passiva se estava ativa
            if (enablePassiveRegen)
            {
                StartCoroutine(PassiveRegeneration());
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

            // REMOVIDO: GameManager.PlayerDied() porque agora não é mais PvE wave-based
            // No MOBA, morte de jogador não afeta o jogo diretamente, só o respawn
        }

        [PunRPC]
        private void RPC_Respawn()
        {
            isDead = false;

            if (playerCollider != null)
                playerCollider.enabled = true;

            foreach (Renderer rend in renderers)
            {
                rend.enabled = true;
            }

            if (controller != null)
                controller.detectCollisions = true;
        }

        // REMOVIDO: RPC_NotifyDeathToMaster (não necessário no sistema MOBA)

        public void SetMaxHealth(int value)
        {
            maxHealth = value;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateHealthText();
        }

        public void EnablePassiveRegen(bool enable)
        {
            enablePassiveRegen = enable;

            if (enable && photonView.IsMine && !isDead)
            {
                StartCoroutine(PassiveRegeneration());
            }
            else
            {
                StopCoroutine(PassiveRegeneration());
            }
        }

        // NOVO: Getter para o time do jogador
        public Team GetTeam()
        {
            return playerTeam;
        }

        // NOVO: Setter para o time (chamado pelo PlayerController no Start)
        public void SetTeam(Team team)
        {
            playerTeam = team;
        }
    }
}