using UnityEngine;
using Photon.Pun;
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

        // REMOVIDO: healthText, deathCanvas, respawnTimerText

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
            // REMOVIDO: UpdateHealthText();
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
            MonoBehaviour weaponSys = GetComponent("WeaponSystem") as MonoBehaviour;
            if (weaponSys != null)
            {
                components.Add(weaponSys);
            }
            controllableComponents = components.ToArray();
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
                    Debug.Log($"[PlayerHealth] Time inicializado: {playerTeam}");
                    yield break;
                }

                elapsed += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
        }

        [PunRPC]
        public void TakeDamage(int damage, int attackerViewID)
        {
            if (isDead) return;

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

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

        // REMOVIDO: UpdateHealthText()

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
            float timer = respawnTime;

            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            Respawn();
        }

        private void Respawn()
        {
            if (!photonView.IsMine) return;

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
            }

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
        }

        [PunRPC]
        private void RPC_ResetHealth()
        {
            currentHealth = maxHealth;
        }

        [PunRPC]
        private void RPC_SetDead()
        {
            if (isDead) return;

            isDead = true;
            if (playerCollider != null)
                playerCollider.enabled = false;
            if (controller != null)
                controller.detectCollisions = false;

            if (photonView.IsMine)
            {
                SetControllableComponents(false);
            }

            if (animationController != null)
            {
                animationController.PlayDeathAnimation();
            }

            StartCoroutine(HideBodyAfterAnimation());
        }

        private IEnumerator HideBodyAfterAnimation()
        {
            yield return new WaitForSeconds(deathAnimationDuration);
            foreach (Renderer rend in renderers)
            {
                rend.enabled = false;
            }
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

            if (photonView.IsMine)
            {
                SetControllableComponents(true);
            }
        }

        private void SetControllableComponents(bool enabled)
        {
            if (controllableComponents == null) return;
            foreach (MonoBehaviour component in controllableComponents)
            {
                if (component != null)
                {
                    component.enabled = enabled;
                }
            }
        }

        public Team GetTeam() => playerTeam;
        public int GetCurrentHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
        public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    }
}