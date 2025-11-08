using Photon.Pun;
using System.Collections;
using UnityEngine;
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

        // REMOVIDO: cooldownSlider, cooldownCanvasGroup

        [Header("Audio")]
        public string attackSoundName = "Spear";

        private bool canAttack = true;
        private Team ownerTeam = Team.None;

        private void OnEnable()
        {
            canAttack = true;

            if (ownerTeam == Team.None)
            {
                StartCoroutine(InitializeTeam());
            }
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
                    yield break;
                }

                elapsed += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void Update()
        {
            if (!photonView.IsMine) return;
            if (!enabled) return;

            if (Input.GetMouseButtonDown(0) && canAttack)
            {
                Attack();
            }
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

            RaycastHit[] hits = Physics.SphereCastAll(attackOrigin, 1.0f, attackDirection, attackRange, damageableLayers);

            if (hits.Length > 0)
            {
                ProcessHits(hits);
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

                        targetsHit++;
                        isFirstHit = false;
                        continue;
                    }
                }
            }
        }

        private IEnumerator StartCooldown()
        {
            yield return new WaitForSeconds(attackCooldown);
            canAttack = true;
        }

        // REMOVIDO: UpdateCooldownUI()

        public bool CanAttack() => canAttack;
        public float GetCooldownProgress() => canAttack ? 1f : 0f; // Simplificado

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