using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MOBAGame.Core;
using MOBAGame.Minions;
using MOBAGame.Player;

namespace MOBAGame.Weapons
{
    public class MeleeWeapon : MonoBehaviourPun
    {
        [Header("Weapon Stats")]
        public int damage = 25;
        public float attackRange = 4f;
        public float attackCooldown = 0.4f;
        public LayerMask damageableLayers;

        [Header("Multi-hit Settings")]
        public bool allowMultiHit = true; // Permite atingir múltiplos alvos
        public int maxTargets = 3; // Máximo de alvos por ataque
        public float damageReductionPerTarget = 0.5f; // Redução de dano após primeiro alvo (50%)

        [Header("Attack Point")]
        public Transform attackPoint; // Ponto de onde sai o raycast/spherecast

        [Header("Cooldown UI")]
        public Slider cooldownSlider;
        public CanvasGroup cooldownCanvasGroup;
        public float fadeSpeed = 5f;

        [Header("Audio")]
        public string attackSoundName = "Spear";

        private bool canAttack = true;
        private bool isFadingIn = false;
        private bool isFadingOut = false;
        private float currentCooldown = 0f;
        private Team ownerTeam = Team.None;

        private void Start()
        {
            // Obtém o time do dono da arma
            if (photonView.Owner != null && photonView.Owner.CustomProperties.TryGetValue("Team", out object teamValue))
            {
                ownerTeam = (Team)((int)teamValue);
            }

            // Configura UI inicial
            if (cooldownSlider != null)
            {
                cooldownSlider.gameObject.SetActive(false);
                cooldownSlider.maxValue = attackCooldown;
                cooldownSlider.value = 0f;
            }

            if (cooldownCanvasGroup != null)
            {
                cooldownCanvasGroup.alpha = 0f;
            }
        }

        private void Update()
        {
            // Apenas o dono da arma pode atacar
            if (!photonView.IsMine) return;

            // Detecta ataque
            if (Input.GetMouseButtonDown(0) && canAttack)
            {
                Attack();
            }

            // Atualiza cooldown UI
            UpdateCooldownUI();
        }

        /// <summary>
        /// Executa o ataque melee (raycast/spherecast para detectar alvos)
        /// </summary>
        private void Attack()
        {
            canAttack = false;

            // Toca som de ataque
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(attackSoundName);
            }

            // Trigger de animação


            // Detecção de alvos
            Vector3 attackOrigin = attackPoint != null ? attackPoint.position : transform.position;
            Vector3 attackDirection = attackPoint != null ? attackPoint.forward : transform.forward;

            // SphereCast para detectar múltiplos alvos
            RaycastHit[] hits = Physics.SphereCastAll(attackOrigin, 0.5f, attackDirection, attackRange, damageableLayers);

            if (hits.Length > 0)
            {
                ProcessHits(hits);
            }


            // Inicia cooldown
            StartCoroutine(StartCooldown());
        }

        /// <summary>
        /// Processa os alvos atingidos aplicando dano
        /// </summary>
        private void ProcessHits(RaycastHit[] hits)
        {
            int targetsHit = 0;
            bool isFirstHit = true;

            foreach (RaycastHit hit in hits)
            {
                // Limita número de alvos
                if (allowMultiHit && targetsHit >= maxTargets)
                    break;

                // Calcula dano (primeiro alvo recebe dano total, demais recebem reduzido)
                int damageAmount = isFirstHit ? damage : Mathf.RoundToInt(damage * damageReductionPerTarget);

                // Verifica se acertou um jogador
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Valida friendly fire
                    if (playerHealth.GetTeam() != ownerTeam && playerHealth.GetTeam() != Team.None)
                    {
                        playerHealth.TakeDamage(damageAmount, photonView);
                        Debug.Log($"[Melee] Causou {damageAmount} de dano em jogador {hit.collider.name}");

                        targetsHit++;
                        isFirstHit = false;
                        continue;
                    }
                }

                // Verifica se acertou um minion
                MinionHealth minionHealth = hit.collider.GetComponent<MinionHealth>();
                if (minionHealth != null)
                {
                    // Valida se é inimigo
                    if (minionHealth.GetTeam() != ownerTeam && minionHealth.GetTeam() != Team.None)
                    {
                        minionHealth.TakeDamage(damageAmount, ownerTeam);
                        Debug.Log($"[Melee] Causou {damageAmount} de dano em minion {hit.collider.name}");

                        targetsHit++;
                        isFirstHit = false;
                        continue;
                    }
                }
            }

            if (targetsHit == 0)
            {
                Debug.Log("[Melee] Ataque não acertou nenhum alvo válido");
            }
        }

        /// <summary>
        /// Inicia o cooldown do ataque
        /// </summary>
        private IEnumerator StartCooldown()
        {
            isFadingIn = true;
            isFadingOut = false;

            if (cooldownSlider != null)
            {
                cooldownSlider.gameObject.SetActive(true);
                cooldownSlider.value = 0f;
            }

            if (cooldownCanvasGroup != null)
            {
                cooldownCanvasGroup.alpha = 0f;
            }

            currentCooldown = 0f;

            yield return new WaitForSeconds(attackCooldown);

            canAttack = true;
            isFadingIn = false;
            isFadingOut = true;
        }

        /// <summary>
        /// Atualiza a UI de cooldown com fade in/out
        /// </summary>
        private void UpdateCooldownUI()
        {
            if (!canAttack)
            {
                currentCooldown += Time.deltaTime;

                if (cooldownSlider != null)
                {
                    cooldownSlider.value = currentCooldown;
                }

                // Fade in
                if (isFadingIn && cooldownCanvasGroup != null)
                {
                    if (cooldownCanvasGroup.alpha < 1)
                    {
                        cooldownCanvasGroup.alpha = Mathf.Lerp(cooldownCanvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
                    }
                }
            }
            else
            {
                // Fade out
                if (isFadingOut && cooldownCanvasGroup != null)
                {
                    if (cooldownCanvasGroup.alpha > 0.01f)
                    {
                        cooldownCanvasGroup.alpha = Mathf.Lerp(cooldownCanvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
                    }
                    else
                    {
                        cooldownCanvasGroup.alpha = 0f;

                        if (cooldownSlider != null)
                        {
                            cooldownSlider.gameObject.SetActive(false);
                        }

                        isFadingOut = false;
                    }
                }
            }
        }

        /// <summary>
        /// Ativa/desativa a arma (chamado pelo WeaponSystem)
        /// </summary>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        /// <summary>
        /// Getter para verificar se pode atacar
        /// </summary>
        public bool CanAttack()
        {
            return canAttack;
        }

        /// <summary>
        /// Debug visual do alcance de ataque (Gizmos no Editor)
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attackPoint.position, 0.5f);
                Gizmos.DrawRay(attackPoint.position, attackPoint.forward * attackRange);
                Gizmos.DrawWireSphere(attackPoint.position + attackPoint.forward * attackRange, 0.5f);
            }
        }
    }
}