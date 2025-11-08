using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using MOBAGame.Lobby;
using MOBAGame.Combat;
using MOBAGame.Core;
using MOBAGame.Player;
using System.Collections.Generic;

namespace MOBAGame.Minions
{
    [RequireComponent(typeof(NavMeshAgent), typeof(PhotonView))]
    public class MinionAI : MonoBehaviourPun, IDamageable
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float detectionRange = 10f;

        [Header("References")]
        [SerializeField] private Team minionTeam;

        private NavMeshAgent agent;
        public Animator animator;
        private float currentHealth;
        private Transform targetBase;
        private Transform currentTarget;
        private float lastAttackTime;
        private bool isDead = false;

        public Team MinionTeam => minionTeam;

        private List<Transform> targetsInRange = new List<Transform>();

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            currentHealth = maxHealth;
        }

        private void Start()
        {
            // Clientes nao-master desabilitam o NavMeshAgent para evitar conflitos
            if (!PhotonNetwork.IsMasterClient)
            {
                agent.enabled = false;
                return;
            }

            FindEnemyBase();
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient || isDead) return;

            Transform nearestEnemy = FindNearestEnemyMinion();

            if (nearestEnemy != null)
            {
                currentTarget = nearestEnemy;
            }
            else
            {
                currentTarget = targetBase;
            }

            if (currentTarget != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

                if (distanceToTarget <= attackRange)
                {
                    agent.isStopped = true;

                    // Rotaciona para o alvo
                    Vector3 direction = (currentTarget.position - transform.position).normalized;
                    direction.y = 0;
                    if (direction != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                    }

                    // Sincroniza animacao de ataque
                    photonView.RPC("RPC_SetAnimation", RpcTarget.All, "IsAttacking", true);
                    AttackTarget();
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(currentTarget.position);

                    // Sincroniza animacao de caminhada
                    photonView.RPC("RPC_SetAnimation", RpcTarget.All, "IsAttacking", false);
                }
            }
        }

        public void OnTargetEnterRange(Transform target)
        {
            if (!targetsInRange.Contains(target))
            {
                targetsInRange.Add(target);
                UpdateCurrentTarget();
            }
        }

        public void OnTargetExitRange(Transform target)
        {
            if (targetsInRange.Contains(target))
            {
                targetsInRange.Remove(target);

                if (currentTarget == target)
                {
                    UpdateCurrentTarget();
                }
            }
        }

        private void UpdateCurrentTarget()
        {
            targetsInRange.RemoveAll(t => t == null);

            if (targetsInRange.Count == 0)
            {
                currentTarget = null;
                return;
            }

            Transform closestBase = null;
            Transform closestMinion = null;
            Transform closestPlayer = null;
            float closestMinionDist = Mathf.Infinity;
            float closestPlayerDist = Mathf.Infinity;

            foreach (Transform target in targetsInRange)
            {
                if (target == null) continue;

                BaseController baseController = target.GetComponent<BaseController>();
                if (baseController != null && baseController.baseTeam != minionTeam)
                {
                    closestBase = target;
                    break;
                }

                MinionHealth minionHealth = target.GetComponent<MinionHealth>();
                if (minionHealth != null && minionHealth.GetTeam() != minionTeam)
                {
                    float dist = Vector3.Distance(transform.position, target.position);
                    if (dist < closestMinionDist)
                    {
                        closestMinionDist = dist;
                        closestMinion = target;
                    }
                }

                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null && playerHealth.GetTeam() != minionTeam)
                {
                    float dist = Vector3.Distance(transform.position, target.position);
                    if (dist < closestPlayerDist)
                    {
                        closestPlayerDist = dist;
                        closestPlayer = target;
                    }
                }
            }

            if (closestBase != null)
                currentTarget = closestBase;
            else if (closestMinion != null)
                currentTarget = closestMinion;
            else if (closestPlayer != null)
                currentTarget = closestPlayer;
            else
                currentTarget = null;
        }

        private void FindEnemyBase()
        {
            BaseController[] bases = FindObjectsOfType<BaseController>();
            foreach (BaseController baseCtrl in bases)
            {
                if (baseCtrl.BaseTeam != minionTeam)
                {
                    targetBase = baseCtrl.transform;
                    break;
                }
            }
        }

        private Transform FindNearestEnemyMinion()
        {
            MinionAI[] minions = FindObjectsOfType<MinionAI>();
            Transform nearest = null;
            float minDistance = detectionRange;

            foreach (MinionAI minion in minions)
            {
                if (minion.MinionTeam != minionTeam && !minion.isDead)
                {
                    float distance = Vector3.Distance(transform.position, minion.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = minion.transform;
                    }
                }
            }

            return nearest;
        }

        private void AttackTarget()
        {
            if (Time.time < lastAttackTime + attackCooldown) return;

            lastAttackTime = Time.time;

            // Sincroniza trigger de ataque
            photonView.RPC("RPC_TriggerAnimation", RpcTarget.All, "Attack");

            IDamageable damageable = currentTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                PhotonView targetView = currentTarget.GetComponent<PhotonView>();
                if (targetView != null)
                {
                    photonView.RPC("RPC_DealDamage", RpcTarget.All, targetView.ViewID, damage);
                }
            }
        }

        [PunRPC]
        private void RPC_SetAnimation(string parameterName, bool value)
        {
            if (animator != null)
            {
                animator.SetBool(parameterName, value);
            }
        }

        [PunRPC]
        private void RPC_TriggerAnimation(string triggerName)
        {
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
        }

        [PunRPC]
        private void RPC_DealDamage(int targetViewID, float damageAmount)
        {
            PhotonView targetView = PhotonView.Find(targetViewID);
            if (targetView != null)
            {
                IDamageable damageable = targetView.GetComponent<IDamageable>();
                damageable?.TakeDamage(damageAmount);
            }
        }

        public void TakeDamage(float damageAmount)
        {
            if (isDead) return;

            // Apenas MasterClient processa dano
            if (!PhotonNetwork.IsMasterClient) return;

            currentHealth -= damageAmount;
            photonView.RPC("RPC_UpdateHealth", RpcTarget.All, currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        [PunRPC]
        private void RPC_UpdateHealth(float newHealth)
        {
            currentHealth = newHealth;
            // Feedback visual aqui se necessario
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            photonView.RPC("RPC_Die", RpcTarget.All);

            if (PhotonNetwork.IsMasterClient)
            {
                Destroy(gameObject, 2f);
            }
        }

        [PunRPC]
        private void RPC_Die()
        {
            isDead = true;
            agent.isStopped = true;

            Collider col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            if (animator != null)
                animator.SetTrigger("Die");
        }
    }
}