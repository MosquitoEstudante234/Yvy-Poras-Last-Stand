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

        [Header("Cooldown UI - External Canvas")]
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

        private void OnEnable()
        {
            // Reseta estado ao equipar
            canAttack = true;

            if (cooldownSlider != null)
                cooldownSlider.gameObject.SetActive(false);

            if (cooldownCanvasGroup != null)
                cooldownCanvasGroup.alpha = 0f;

            // Inicializa time se ainda nao foi feito
            if (ownerTeam == Team.None)
            {
                StartCoroutine(InitializeTeam());
            }
        }

        private void OnDisable()
        {
            // Esconde UI ao desequipar
            if (cooldownSlider != null)
                cooldownSlider.gameObject.SetActive(false);

            if (cooldownCanvasGroup != null)
                cooldownCanvasGroup.alpha = 0f;
        }

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
            if (!photonView.IsMine) return;
            if (!enabled) return; // Nao ataca se script desabilitado

            if (Input.GetMouseButtonDown(0) && canAttack)
            {
                Attack();
            }

            UpdateCooldownUI();
        }

        private void Attack()
        {
            canAttack = false;

            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(attackSoundName);
            }

            Vector3 attackOrigin = attackPoint != null ? attackPoint.position : transform.position;
            Vector3 attackDirection = attackPoint != null ? attackPoint.forward : transform.forward;

            // SphereCast com raio de 1.0 (aumentado para melhor deteccao)
            RaycastHit[] hits = Physics.SphereCastAll(attackOrigin, 1.0f, attackDirection, attackRange, damageableLayers);

            if (hits.Length > 0)
            {
                ProcessHits(hits);
            }
            else
            {
                Debug.Log("[Melee] Ataque nao detectou nenhum collider");
            }

            StartCoroutine(StartCooldown());
        }

        private void ProcessHits(RaycastHit[] hits)
        {
            int targetsHit = 0;
            bool isFirstHit = true;

            foreach (RaycastHit hit in hits)
            {
                if (allowMultiHit && targetsHit >= maxTargets)
                    break;

                int damageAmount = isFirstHit ? damage : Mathf.RoundToInt(damage * damageReductionPerTarget);

                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    if (playerHealth.GetTeam() != ownerTeam && playerHealth.GetTeam() != Team.None)
                    {
                        PhotonView targetPhotonView = playerHealth.GetComponent<PhotonView>();
                        if (targetPhotonView != null)
                        {
                            targetPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, damageAmount, photonView.ViewID);
                            Debug.Log($"[Melee] Causou {damageAmount} de dano em {hit.collider.name}");
                        }

                        targetsHit++;
                        isFirstHit = false;
                        continue;
                    }
                }

                MinionHealth minionHealth = hit.collider.GetComponent<MinionHealth>();
                if (minionHealth != null)
                {
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
                Debug.Log("[Melee] Nenhum alvo valido atingido");
            }
        }

        private IEnumerator StartCooldown()
        {
            isFadingIn = true;
            isFadingOut = false;

            if (cooldownSlider != null)
            {
                cooldownSlider.gameObject.SetActive(true);
                cooldownSlider.maxValue = attackCooldown;
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

        private void UpdateCooldownUI()
        {
            if (!canAttack)
            {
                currentCooldown += Time.deltaTime;

                if (cooldownSlider != null)
                {
                    cooldownSlider.value = currentCooldown;
                }

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

        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attackPoint.position, 1.0f);
                Gizmos.DrawRay(attackPoint.position, attackPoint.forward * attackRange);
                Gizmos.DrawWireSphere(attackPoint.position + attackPoint.forward * attackRange, 1.0f);
            }
        }
    }
}