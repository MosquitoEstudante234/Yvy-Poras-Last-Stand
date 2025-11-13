using System.Collections;
using UnityEngine;
using Photon.Pun;
using MOBAGame.Core;
using MOBAGame.Player;

namespace MOBAGame.Minions
{
    public class MinionHealth : MonoBehaviourPun
    {
        [Header("Health Settings")]
        public int maxHealth = 50;
        private int currentHealth;

        [Header("Combat Settings")]
        public int damage = 10;
        public float attackCooldown = 2f;
        private float lastAttackTime = 0f;

        [Header("Team")]
        public Team minionTeam = Team.None;

        [Header("Death Settings")]
        [SerializeField] private float deathAnimationDuration = 2f;

        [Header("Visual Feedback")]
        private Renderer minionRenderer;
        private Color originalColor;

        [Header("Audio")]
        public string hitSoundName = "HitEnemy";
        public string deathSoundName = "EnemyDeath";

        public delegate void DeathDelegate();
        public event DeathDelegate OnDeath;

        private bool isDead = false;

        private void Start()
        {
            currentHealth = maxHealth;

            minionRenderer = GetComponent<Renderer>();
            if (minionRenderer != null)
            {
                // Cria material único para este minion (evita compartilhamento)
                minionRenderer.material = new Material(minionRenderer.material);
                originalColor = minionRenderer.material.color;

                // Define cor baseada no time
                SetTeamColor();
            }
        }

        /// <summary>
        /// Define a cor visual do minion baseado no time
        /// </summary>
        private void SetTeamColor()
        {
            if (minionRenderer == null) return;

            switch (minionTeam)
            {
                case Team.Indigenous:
                    // Tom verde/marrom para indígenas
                    minionRenderer.material.color = new Color(0.4f, 0.6f, 0.3f);
                    break;
                case Team.Portuguese:
                    // Tom azul/cinza para portugueses
                    minionRenderer.material.color = new Color(0.3f, 0.4f, 0.6f);
                    break;
            }

            originalColor = minionRenderer.material.color;
        }

        /// <summary>
        /// Recebe dano de jogador ou outro minion
        /// </summary>
        public void TakeDamage(int damageAmount, Team attackerTeam)
        {
            if (isDead) return;

            // Validação: não pode receber dano do mesmo time
            if (attackerTeam == minionTeam && attackerTeam != Team.None)
            {
                Debug.Log($"[MinionHealth] Dano de friendly fire ignorado");
                return;
            }

            // Sincroniza dano via RPC
            photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damageAmount);
        }

        [PunRPC]
        private void RPC_TakeDamage(int damageAmount)
        {
            if (isDead) return;

            currentHealth -= damageAmount;

            Debug.Log($"[MinionHealth] {gameObject.name} recebeu {damageAmount} de dano. HP: {currentHealth}/{maxHealth}");

            StartCoroutine(FlashRed());

            // Toca som de hit
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(hitSoundName);
            }

            // Apenas o MasterClient processa a morte
            if (currentHealth <= 0 && PhotonNetwork.IsMasterClient && !isDead)
            {
                Debug.Log($"[MinionHealth] {gameObject.name} morreu! Chamando Die()");
                Die();
            }
        }

        /// <summary>
        /// Efeito visual de feedback ao receber dano
        /// </summary>
        private IEnumerator FlashRed()
        {
            if (minionRenderer != null)
            {
                minionRenderer.material.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                minionRenderer.material.color = originalColor;
            }
        }

        /// <summary>
        /// Morte do minion
        /// </summary>
        private void Die()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log($"[MinionHealth] Die() chamado no MasterClient para {gameObject.name}");

            // Invoca evento de morte (para MinionAI descontar do limite de spawns)
            OnDeath?.Invoke();

            // Toca som de morte via RPC para todos ouvirem
            photonView.RPC(nameof(RPC_PlayDeathSound), RpcTarget.All);

            // Aguarda animação antes de destruir
            StartCoroutine(DestroyAfterAnimation());
        }

        [PunRPC]
        private void RPC_PlayDeathSound()
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(deathSoundName);
            }
        }

        /// <summary>
        /// Aguarda animação de morte antes de destruir o GameObject
        /// </summary>
        private IEnumerator DestroyAfterAnimation()
        {
            Debug.Log($"[MinionHealth] Aguardando {deathAnimationDuration}s para destruir {gameObject.name}");

            yield return new WaitForSeconds(deathAnimationDuration);

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"[MinionHealth] Destruindo {gameObject.name} via PhotonNetwork.Destroy");
                PhotonNetwork.Destroy(gameObject);
            }
        }

        /// <summary>
        /// Ataca um alvo (Base ou outro Minion)
        /// </summary>
        public void AttackTarget(GameObject target)
        {
            if (Time.time < lastAttackTime + attackCooldown)
                return;

            lastAttackTime = Time.time;

            // Verifica se é uma Base
            BaseController baseController = target.GetComponent<BaseController>();
            if (baseController != null)
            {
                // Apenas MasterClient processa dano em bases
                if (PhotonNetwork.IsMasterClient)
                {
                    baseController.TakeDamage(damage);
                }
                return;
            }

            // Verifica se é outro Minion
            MinionHealth enemyMinion = target.GetComponent<MinionHealth>();
            if (enemyMinion != null)
            {
                // Valida se é inimigo
                if (enemyMinion.minionTeam != this.minionTeam)
                {
                    enemyMinion.TakeDamage(damage, this.minionTeam);
                }
                return;
            }

            // OPCIONAL: Se quiser que minions possam atacar jogadores também
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (playerHealth.GetTeam() != this.minionTeam)
                {
                    playerHealth.TakeDamage(damage, this.photonView);
                }
            }
        }

        /// <summary>
        /// Getter do time do minion
        /// </summary>
        public Team GetTeam()
        {
            return minionTeam;
        }

        /// <summary>
        /// Setter do time do minion (chamado pelo MinionSpawner)
        /// </summary>
        public void SetTeam(Team team)
        {
            minionTeam = team;
            SetTeamColor();
        }

        /// <summary>
        /// Verifica se o minion está morto
        /// </summary>
        public bool IsDead()
        {
            return isDead;
        }

        /// <summary>
        /// Retorna a vida atual
        /// </summary>
        public int GetCurrentHealth()
        {
            return currentHealth;
        }

        /// <summary>
        /// Retorna porcentagem de vida (0 a 1)
        /// </summary>
        public float GetHealthPercentage()
        {
            return (float)currentHealth / maxHealth;
        }
    }
}