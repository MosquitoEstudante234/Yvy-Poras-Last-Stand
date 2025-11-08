using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
using MOBAGame.Core;

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
        public TextMeshProUGUI respawnTimerText;

        [Header("Passive Regen Settings")]
        public bool enablePassiveRegen = false;
        public float regenRate = 1f;
        public float regenInterval = 1f;
        private Coroutine regenCoroutine;

        [Header("Respawn Settings")]
        public float respawnTime = 7f;
        private Coroutine respawnCoroutine; // NOVO: Guarda referência para evitar múltiplas coroutines

        [Header("Visual Feedback")]
        public Renderer playerRenderer;
        private Color originalColor;
        private Material playerMaterial;

        private PlayerAnimationController animationController;
        private Team playerTeam = Team.None;

        private void Start()
        {
            animationController = GetComponent<PlayerAnimationController>();
            currentHealth = maxHealth;
            controller = GetComponent<CharacterController>();
            playerCollider = GetComponent<Collider>();
            renderers = GetComponentsInChildren<Renderer>();

            if (deathCanvas != null)
                deathCanvas.SetActive(false);

            UpdateHealthText();

            StartCoroutine(InitializeTeam());

            if (playerRenderer != null)
            {
                playerMaterial = playerRenderer.material;
                originalColor = playerMaterial.color;
            }

            if (photonView.IsMine)
            {
                PhotonNetwork.LocalPlayer.TagObject = this;

                if (enablePassiveRegen)
                {
                    regenCoroutine = StartCoroutine(PassiveRegeneration());
                }
            }
        }

        private IEnumerator InitializeTeam()
        {
            float timeout = 5f;
            float elapsed = 0f;

            while (playerTeam == Team.None && elapsed < timeout)
            {
                if (photonView.Owner != null && photonView.Owner.CustomProperties.TryGetValue("Team", out object teamValue))
                {
                    playerTeam = (Team)((int)teamValue);
                    Debug.Log($"[PlayerHealth] Time inicializado: {playerTeam} (Player: {photonView.Owner.NickName})");
                    yield break;
                }

                elapsed += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            if (playerTeam == Team.None)
            {
                Debug.LogError($"[PlayerHealth] ERRO: Falha ao obter time do owner: {photonView.Owner?.NickName}");
            }
        }

        /// <summary>
        ///  CORRIGIDO: Agora sincroniza a vida via RPC
        /// </summary>
        [PunRPC]
        public void TakeDamage(int damage, int attackerViewID)
        {
            if (isDead)
            {
                Debug.Log($"[PlayerHealth] Dano ignorado - jogador já está morto: {photonView.Owner.NickName}");
                return;
            }

            // Aplica dano (executa em TODOS os clientes via RPC)
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            PhotonView attackerView = PhotonView.Find(attackerViewID);
            string attackerName = attackerView != null ? attackerView.Owner.NickName : "Desconhecido";

            Debug.Log($"[PlayerHealth] {photonView.Owner.NickName} recebeu {damage} de dano de {attackerName}. HP: {currentHealth}/{maxHealth}");

            // Atualiza UI (apenas no owner)
            if (photonView.IsMine)
            {
                UpdateHealthText();
                StartCoroutine(FlashDamage());
            }

            // Verifica morte
            if (currentHealth <= 0 && !isDead) // Adiciona verificação !isDead
            {
                photonView.RPC(nameof(RPC_SetDead), RpcTarget.All);

                // Inicia respawn APENAS no owner e cancela respawn anterior
                if (photonView.IsMine)
                {
                    if (respawnCoroutine != null)
                    {
                        StopCoroutine(respawnCoroutine);
                    }
                    Debug.Log($"[PlayerHealth] Iniciando countdown de respawn para {photonView.Owner.NickName}");
                    respawnCoroutine = StartCoroutine(RespawnCountdown());
                }
            }
        }

        /// <summary>
        /// MÉTODO LEGADO (mantido para compatibilidade)
        /// </summary>
        public void TakeDamage(int damage, PhotonView attacker = null)
        {
            if (!photonView.IsMine || isDead) return;

            if (attacker != null && attacker.Owner != null)
            {
                if (attacker.Owner.CustomProperties.TryGetValue("Team", out object attackerTeamValue))
                {
                    Team attackerTeam = (Team)((int)attackerTeamValue);
                    if (attackerTeam == playerTeam && attackerTeam != Team.None)
                    {
                        Debug.Log($"[PlayerHealth] Friendly fire ignorado de {attacker.Owner.NickName}");
                        return;
                    }
                }
            }

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateHealthText();

            if (currentHealth <= 0 && !isDead)
            {
                photonView.RPC(nameof(RPC_SetDead), RpcTarget.All);

                if (photonView.IsMine)
                {
                    if (respawnCoroutine != null)
                    {
                        StopCoroutine(respawnCoroutine);
                    }
                    respawnCoroutine = StartCoroutine(RespawnCountdown());
                }
            }
        }

        private IEnumerator FlashDamage()
        {
            if (playerMaterial != null)
            {
                playerMaterial.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                playerMaterial.color = originalColor;
            }
        }

        private void UpdateHealthText()
        {
            if (healthText != null)
            {
                healthText.text = "Vida: " + currentHealth.ToString();
                Debug.Log($"[PlayerHealth] UI atualizada: {currentHealth}/{maxHealth}");
            }
        }

        private IEnumerator PassiveRegeneration()
        {
            while (!isDead)
            {
                yield return new WaitForSeconds(regenInterval);

                if (currentHealth < maxHealth && !isDead)
                {
                    currentHealth += Mathf.RoundToInt(regenRate);
                    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                    UpdateHealthText();
                }
            }
        }

        /// <summary>
        ///  Sistema de countdown CORRIGIDO
        /// </summary>
        private IEnumerator RespawnCountdown()
        {
            Debug.Log($"[PlayerHealth] RespawnCountdown iniciado para {photonView.Owner.NickName}");
            float timer = respawnTime;

            while (timer > 0)
            {
                if (respawnTimerText != null)
                {
                    respawnTimerText.text = $"Respawn em: {Mathf.CeilToInt(timer)}s";
                }

                Debug.Log($"[PlayerHealth] Respawn em: {Mathf.CeilToInt(timer)}s");
                timer -= Time.deltaTime;
                yield return null;
            }

            Debug.Log($"[PlayerHealth] Countdown finalizado! Chamando Respawn()");
            Respawn();
        }

        /// <summary>
        /// Sistema de respawn COMPLETAMENTE CORRIGIDO
        /// </summary>
        private void Respawn()
        {
            if (!photonView.IsMine)
            {
                Debug.LogWarning($"[PlayerHealth] Respawn chamado mas não é o owner!");
                return;
            }

            Debug.Log($"[PlayerHealth] Iniciando respawn de {photonView.Owner.NickName}");

            // Reseta vida VIA RPC para sincronizar entre todos os clientes
            photonView.RPC(nameof(RPC_ResetHealth), RpcTarget.All);

            // Reativa componentes via RPC
            photonView.RPC(nameof(RPC_Respawn), RpcTarget.All);

            // Teleporta para spawn point do time
            Transform spawnPoint = null;

            if (GameManager.instance != null)
            {
                spawnPoint = GameManager.instance.GetSpawnPointForTeam(playerTeam);
            }

            if (spawnPoint != null)
            {
                controller.enabled = false;
                transform.position = spawnPoint.position;
                transform.rotation = spawnPoint.rotation;
                controller.enabled = true;
                Debug.Log($"[PlayerHealth] {photonView.Owner.NickName} respawnou no spawn do time {playerTeam}");
            }
            else
            {
                Vector3 fallbackPosition = playerTeam == Team.Indigenous ? new Vector3(0, 1, -10) : new Vector3(0, 1, 10);
                controller.enabled = false;
                transform.position = fallbackPosition;
                transform.rotation = Quaternion.identity;
                controller.enabled = true;
                Debug.LogWarning($"[PlayerHealth] GameManager não encontrado! Respawn em posição fallback: {fallbackPosition}");
            }

            // Esconde canvas de morte
            if (deathCanvas != null)
                deathCanvas.SetActive(false);

            // Reinicia regeneração passiva
            if (enablePassiveRegen)
            {
                if (regenCoroutine != null)
                {
                    StopCoroutine(regenCoroutine);
                }
                regenCoroutine = StartCoroutine(PassiveRegeneration());
            }

            // Reseta animação de morte
            if (animationController != null)
            {
                animationController.ResetDeathAnimation();
            }

            // Limpa referência da coroutine de respawn
            respawnCoroutine = null;

            Debug.Log($"[PlayerHealth] Respawn de {photonView.Owner.NickName} concluído! HP: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// NOVO RPC: Reseta a vida sincronizando entre todos os clientes
        /// </summary>
        [PunRPC]
        private void RPC_ResetHealth()
        {
            currentHealth = maxHealth;

            // Atualiza UI apenas no owner
            if (photonView.IsMine)
            {
                UpdateHealthText();
            }

            Debug.Log($"[PlayerHealth] RPC_ResetHealth: {photonView.Owner.NickName} HP resetado para {currentHealth}/{maxHealth}");
        }

        [PunRPC]
        private void RPC_SetDead()
        {
            if (isDead)
            {
                Debug.LogWarning($"[PlayerHealth] RPC_SetDead chamado mas já está morto!");
                return;
            }

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

            if (animationController != null)
            {
                animationController.PlayDeathAnimation();
            }

            Debug.Log($"[PlayerHealth] {photonView.Owner.NickName} morreu! isDead={isDead}");
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

            Debug.Log($"[PlayerHealth] RPC_Respawn executado para {photonView.Owner.NickName}. isDead={isDead}");
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

            if (enable && photonView.IsMine && !isDead)
            {
                if (regenCoroutine != null)
                {
                    StopCoroutine(regenCoroutine);
                }
                regenCoroutine = StartCoroutine(PassiveRegeneration());
            }
            else
            {
                if (regenCoroutine != null)
                {
                    StopCoroutine(regenCoroutine);
                    regenCoroutine = null;
                }
            }
        }

        public Team GetTeam()
        {
            return playerTeam;
        }

        public void SetTeam(Team team)
        {
            playerTeam = team;
            Debug.Log($"[PlayerHealth] Time definido manualmente: {team}");
        }

        public int GetCurrentHealth()
        {
            return currentHealth;
        }

        public int GetMaxHealth()
        {
            return maxHealth;
        }

        public float GetHealthPercentage()
        {
            return (float)currentHealth / maxHealth;
        }
    }
}