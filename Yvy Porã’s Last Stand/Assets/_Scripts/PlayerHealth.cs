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

        [Header("Death UI (Filho do Player)")]
        public GameObject deathCanvas;
        public TextMeshProUGUI respawnTimerText;

        [Header("Passive Regen Settings")]
        public bool enablePassiveRegen = false;
        public float regenRate = 1f;
        public float regenInterval = 1f;
        private Coroutine regenCoroutine;

        [Header("Respawn Settings")]
        public float respawnTime = 7f;
        public float deathAnimationDuration = 2f;
        private Coroutine respawnCoroutine;

        [Header("Visual Feedback")]
        public Renderer playerRenderer;
        private Color originalColor;
        private Material playerMaterial;

        private PlayerAnimationController animationController;
        private Team playerTeam = Team.None;
        private MonoBehaviour[] controllableComponents;

        private void Start()
        {
            animationController = GetComponent<PlayerAnimationController>();
            currentHealth = maxHealth;
            controller = GetComponent<CharacterController>();
            playerCollider = GetComponent<Collider>();
            renderers = GetComponentsInChildren<Renderer>();

            CacheControllableComponents();

            if (deathCanvas != null)
                deathCanvas.SetActive(false);

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

        private void CacheControllableComponents()
        {
            System.Collections.Generic.List<MonoBehaviour> components = new System.Collections.Generic.List<MonoBehaviour>();

            MonoBehaviour fpsController = GetComponent("FirstPersonController") as MonoBehaviour;
            if (fpsController != null)
            {
                components.Add(fpsController);
            }

            MonoBehaviour weaponSys = GetComponent("WeaponSystem") as MonoBehaviour;
            if (weaponSys != null)
            {
                components.Add(weaponSys);
            }

            controllableComponents = components.ToArray();
            Debug.Log($"[PlayerHealth] {controllableComponents.Length} componentes de controle encontrados");
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

        [PunRPC]
        public void TakeDamage(int damage, int attackerViewID)
        {
            if (isDead)
            {
                Debug.Log($"[PlayerHealth] Dano ignorado - jogador ja esta morto: {photonView.Owner.NickName}");
                return;
            }

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            PhotonView attackerView = PhotonView.Find(attackerViewID);
            string attackerName = attackerView != null ? attackerView.Owner.NickName : "Desconhecido";

            Debug.Log($"[PlayerHealth] {photonView.Owner.NickName} recebeu {damage} de dano de {attackerName}. HP: {currentHealth}/{maxHealth}");

            if (photonView.IsMine)
            {
                StartCoroutine(FlashDamage());
            }

            if (currentHealth <= 0 && !isDead)
            {
                photonView.RPC(nameof(RPC_SetDead), RpcTarget.All);

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

        private IEnumerator PassiveRegeneration()
        {
            while (!isDead)
            {
                yield return new WaitForSeconds(regenInterval);

                if (currentHealth < maxHealth && !isDead)
                {
                    currentHealth += Mathf.RoundToInt(regenRate);
                    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                }
            }
        }

        private IEnumerator RespawnCountdown()
        {
            Debug.Log($"[PlayerHealth] RespawnCountdown iniciado para {photonView.Owner.NickName}");
            float timer = respawnTime;

            if (deathCanvas != null)
                deathCanvas.SetActive(true);

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

        private void Respawn()
        {
            if (!photonView.IsMine)
            {
                Debug.LogWarning($"[PlayerHealth] Respawn chamado mas nao e o owner!");
                return;
            }

            Debug.Log($"[PlayerHealth] Iniciando respawn de {photonView.Owner.NickName}");

            photonView.RPC(nameof(RPC_ResetHealth), RpcTarget.All);
            photonView.RPC(nameof(RPC_Respawn), RpcTarget.All);

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
                Debug.LogWarning($"[PlayerHealth] GameManager nao encontrado! Respawn em posicao fallback: {fallbackPosition}");
            }

            if (deathCanvas != null)
                deathCanvas.SetActive(false);

            if (enablePassiveRegen)
            {
                if (regenCoroutine != null)
                {
                    StopCoroutine(regenCoroutine);
                }
                regenCoroutine = StartCoroutine(PassiveRegeneration());
            }

            if (animationController != null)
            {
                animationController.ResetDeathAnimation();
            }

            respawnCoroutine = null;

            Debug.Log($"[PlayerHealth] Respawn de {photonView.Owner.NickName} concluido! HP: {currentHealth}/{maxHealth}");
        }

        [PunRPC]
        private void RPC_ResetHealth()
        {
            currentHealth = maxHealth;
            Debug.Log($"[PlayerHealth] RPC_ResetHealth: {photonView.Owner.NickName} HP resetado para {currentHealth}/{maxHealth}");
        }

        [PunRPC]
        private void RPC_SetDead()
        {
            if (isDead)
            {
                Debug.LogWarning($"[PlayerHealth] RPC_SetDead chamado mas ja esta morto!");
                return;
            }

            isDead = true;

            if (playerCollider != null)
                playerCollider.enabled = false;

            if (controller != null)
                controller.detectCollisions = false;

            // Desabilita controles do jogador (apenas no owner)
            if (photonView.IsMine)
            {
                SetControllableComponents(false);

                // NOVO: Desativa armas
                Combat.WeaponSystem weaponSystem = GetComponent<Combat.WeaponSystem>();
                if (weaponSystem != null)
                {
                    weaponSystem.DisableAllWeapons();
                }
            }

            // Toca animacao de morte
            if (animationController != null)
            {
                animationController.PlayDeathAnimation();
            }

            // Aguarda animacao de morte antes de esconder o corpo
            StartCoroutine(HideBodyAfterAnimation());

            Debug.Log($"[PlayerHealth] {photonView.Owner.NickName} morreu! isDead={isDead}");
        }

        private IEnumerator HideBodyAfterAnimation()
        {
            Debug.Log($"[PlayerHealth] Aguardando {deathAnimationDuration}s para esconder corpo");
            yield return new WaitForSeconds(deathAnimationDuration);

            foreach (Renderer rend in renderers)
            {
                rend.enabled = false;
            }

            Debug.Log($"[PlayerHealth] Corpo escondido apos animacao");
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

            // Reativa controles do jogador (apenas no owner)
            if (photonView.IsMine)
            {
                SetControllableComponents(true);

                // NOVO: Reativa armas
                Combat.WeaponSystem weaponSystem = GetComponent<Combat.WeaponSystem>();
                if (weaponSystem != null)
                {
                    weaponSystem.EnableCurrentWeapon();
                }
            }

            Debug.Log($"[PlayerHealth] RPC_Respawn executado para {photonView.Owner.NickName}. isDead={isDead}");
        }

        private void SetControllableComponents(bool enabled)
        {
            if (controllableComponents == null) return;

            foreach (MonoBehaviour component in controllableComponents)
            {
                if (component != null)
                {
                    component.enabled = enabled;
                    Debug.Log($"[PlayerHealth] Componente {component.GetType().Name} {(enabled ? "ativado" : "desativado")}");
                }
            }
        }

        public void SetMaxHealth(int value)
        {
            maxHealth = value;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
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

        public Team GetTeam() => playerTeam;
        public void SetTeam(Team team)
        {
            playerTeam = team;
            Debug.Log($"[PlayerHealth] Time definido manualmente: {team}");
        }

        public int GetCurrentHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
        public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    }
}