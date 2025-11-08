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

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            currentHealth = maxHealth;
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
                GetComponent<MinionHealth>().AttackTarget(currentTarget.gameObject);
            }
        }

        // Adicione estas variáveis no topo da classe MinionAI
        private List<Transform> targetsInRange = new List<Transform>();

        /// <summary>
        /// Chamado quando um alvo entra no alcance de detecção
        /// </summary>
        public void OnTargetEnterRange(Transform target)
        {
            if (!targetsInRange.Contains(target))
            {
                targetsInRange.Add(target);
                UpdateCurrentTarget();
            }
        }

        /// <summary>
        /// Chamado quando um alvo sai do alcance de detecção
        /// </summary>
        public void OnTargetExitRange(Transform target)
        {
            if (targetsInRange.Contains(target))
            {
                targetsInRange.Remove(target);

                // Se o alvo atual saiu, busca novo alvo
                if (currentTarget == target)
                {
                    UpdateCurrentTarget();
                }
            }
        }

        /// <summary>
        /// Atualiza o alvo atual com base na lista de alvos no alcance
        /// </summary>
        private void UpdateCurrentTarget()
        {
            // Remove alvos nulos (destruídos)
            targetsInRange.RemoveAll(t => t == null);

            if (targetsInRange.Count == 0)
            {
                currentTarget = null;
                return;
            }

            // Prioridade de alvos:
            // 1. Base inimiga (se estiver no alcance)
            // 2. Minion inimigo mais próximo
            // 3. Jogador inimigo mais próximo

            Transform closestBase = null;
            Transform closestMinion = null;
            Transform closestPlayer = null;
            float closestMinionDist = Mathf.Infinity;
            float closestPlayerDist = Mathf.Infinity;

            foreach (Transform target in targetsInRange)
            {
                if (target == null) continue;

                // Verifica se é uma base
                BaseController baseController = target.GetComponent<BaseController>();
                if (baseController != null && baseController.baseTeam != minionTeam)
                {
                    closestBase = target;
                    break; // Base tem prioridade máxima
                }

                // Verifica se é um minion
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

                // Verifica se é um jogador
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

            // Define alvo com base na prioridade
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

            // Aplicar dano
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

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            photonView.RPC("RPC_Die", RpcTarget.All);

            if (PhotonNetwork.IsMasterClient)
            {
                // Destruir após animação de morte
                Destroy(gameObject, 2f);
            }
        }

        [PunRPC]
        private void RPC_Die()
        {
            isDead = true;
            agent.isStopped = true;
            GetComponent<Collider>().enabled = false;
            animator?.SetTrigger("Die");
        }
    }
}