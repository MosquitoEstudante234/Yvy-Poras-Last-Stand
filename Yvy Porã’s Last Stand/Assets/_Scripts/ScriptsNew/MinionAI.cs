using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using MOBAGame.Lobby;
using MOBAGame.Combat;
using MOBAGame.Core;
using MOBAGame.Player;
using System.Collections.Generic;
using System.Collections;

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

        [Header("Death Settings")]
        [SerializeField] private float deathAnimationDuration = 2f;

        [Header("References")]
        [SerializeField] private Team minionTeam;

        private NavMeshAgent agent;
        public Animator animator;
        private float currentHealth;
        private Transform targetBase;
        private Transform currentTarget;
        private float lastAttackTime;
        private bool isDead = false;
        private MinionHealth minionHealth;

        public Team MinionTeam => minionTeam;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            minionHealth = GetComponent<MinionHealth>();
            currentHealth = maxHealth;

            // Subscreve ao evento de morte do MinionHealth
            if (minionHealth != null)
            {
                minionHealth.OnDeath += HandleDeath;
            }
        }

        private void Start()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // Encontrar base inimiga
            FindEnemyBase();
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient || isDead) return;

            // Procurar minions inimigos próximos
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
                    // Atacar
                    agent.isStopped = true;
                    animator?.SetBool("IsAttacking", true);
                    AttackTarget();
                }
                else
                {
                    // Mover em direção ao alvo
                    agent.isStopped = false;
                    agent.SetDestination(currentTarget.position);
                    animator?.SetBool("IsAttacking", false);
                }
            }

            // Dentro do Update() do MinionAI, quando detectar um alvo próximo:
            if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) < attackRange)
            {
                agent.isStopped = true;
                MinionHealth minionHealthComponent = GetComponent<MinionHealth>();
                if (minionHealthComponent != null)
                {
                    minionHealthComponent.AttackTarget(currentTarget.gameObject);
                }
            }
        }

        private List<Transform> targetsInRange = new List<Transform>();

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

                MinionHealth minionHealthComponent = target.GetComponent<MinionHealth>();
                if (minionHealthComponent != null && minionHealthComponent.GetTeam() != minionTeam)
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
            animator?.SetTrigger("Attack");

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
            // Feedback visual (flash vermelho, etc)
        }

        /// <summary>
        /// Chamado quando MinionHealth detecta morte
        /// </summary>
        private void HandleDeath()
        {
            if (isDead) return;

            Debug.Log("[MinionAI] HandleDeath chamado pelo evento de MinionHealth");
            Die();
        }

        private void Die()
        {
            if (isDead) return;

            // Verifica se MinionHealth já está processando a morte
            if (minionHealth != null && minionHealth.IsDead())
            {
                Debug.Log("[MinionAI] MinionHealth já está processando a morte");
                // Apenas sincroniza visual
                photonView.RPC("RPC_Die", RpcTarget.All);
                return;
            }

            // CRÍTICO: Só o MasterClient deve chamar Die()
            if (!PhotonNetwork.IsMasterClient) return;

            isDead = true;

            Debug.Log("[MinionAI] Die() chamado no MasterClient");

            // Sincroniza morte para todos os clientes
            photonView.RPC("RPC_Die", RpcTarget.All);

            // Nota: MinionHealth cuida da destruição após animação
        }

        [PunRPC]
        private void RPC_Die()
        {
            isDead = true;

            // Para o agente
            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            // Desabilita collider
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            // Toca animação de morte
            if (animator != null)
            {
                animator.SetBool("IsAttacking", false);
                animator.SetTrigger("Die");
                Debug.Log("[MinionAI] Animacao de morte disparada");
            }
            else
            {
                Debug.LogWarning("[MinionAI] Animator nao encontrado - animacao de morte nao tocou!");
            }
        }

        /// <summary>
        /// Getter público para verificar se está morto
        /// </summary>
        public bool IsDead() => isDead;
    }
}