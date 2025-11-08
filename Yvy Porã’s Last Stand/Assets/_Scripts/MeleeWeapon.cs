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
        public bool allowMultiHit = true;
        public int maxTargets = 3;
        public float damageReductionPerTarget = 0.5f;

        [Header("Attack Point")]
        public Transform attackPoint;

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
            // Aguarda sincronização de Custom Properties
            StartCoroutine(InitializeTeam());

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

        /// <summary>
        /// Inicializa o time com delay para garantir sincronização
        /// </summary>
        private IEnumerator InitializeTeam()
        {
            float timeout = 5f;
            float elapsed = 0f;

            while (ownerTeam == Team.None && elapsed < timeout)
            {
                if (photonView.Owner != null && photonView.Owner.CustomProperties.TryGetValue("Team", out object teamValue))
                {
                    ownerTeam = (Team)((int)teamValue);
                    Debug.Log($"[MeleeWeapon] Time inicializado: {ownerTeam}");
                    yield break;
                }

                elapsed += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            if (ownerTeam == Team.None)
            {
                Debug.LogError("[MeleeWeapon] Falha ao obter time do owner!");
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
        /// Executa o ataque melee (spherecast para detectar alvos)
        /// </summary>
        private void Attack()
        {
            canAttack = false;

            // Toca som de ataque
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(attackSoundName);
            }

            // TODO: Trigger de animação (se houver Animator)
            // animator.SetTrigger("Attack");

            // Detecção de alvos
            Vector3 attackOrigin = attackPoint != null ? attackPoint.position : transform.position;
            Vector3 attackDirection = attackPoint != null ? attackPoint.forward : transform.forward;

            // SphereCast para detectar múltiplos alvos
            RaycastHit[] hits = Physics.SphereCastAll(attackOrigin, 0.5f, attackDirection, attackRange, damageableLayers);

            if (hits.Length > 0)
            {
                ProcessHits(hits);
            }
            else
            {
                Debug.Log("[Melee] Ataque não detectou nenhum collider na área");
            }

            // Inicia cooldown
            StartCoroutine(StartCooldown());
        }

        /// <summary>
        /// Processa os alvos atingidos aplicando dano (COM SINCRONIZAÇÃO RPC)
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

                // ========== VERIFICA JOGADORES ==========
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Valida friendly fire
                    if (playerHealth.GetTeam() != ownerTeam && playerHealth.GetTeam() != Team.None)
                    {
                        //  CORREÇÃO: Usa RPC através do PhotonView do alvo
                        PhotonView targetPhotonView = playerHealth.GetComponent<PhotonView>();
                        if (targetPhotonView != null)
                        {
                            // Envia RPC para todos os clientes aplicarem dano
                            targetPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, damageAmount, photonView.ViewID);
                            Debug.Log($"[Melee] RPC enviado: {damageAmount} dano para jogador {hit.collider.name} (ViewID: {targetPhotonView.ViewID})");
                        }
                        else
                        {
                            Debug.LogError($"[Melee] PlayerHealth sem PhotonView: {hit.collider.name}");
                        }

                        targetsHit++;
                        isFirstHit = false;
                        continue;
                    }
                    else
                    {
                        Debug.Log($"[Melee] Ignorado (mesmo time ou Team.None): {hit.collider.name}");
                    }
                }

                // ========== VERIFICA MINIONS ==========
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
                Debug.Log("[Melee] Ataque não acertou nenhum alvo válido (verificar layers e teams)");
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