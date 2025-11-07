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

        [Header("Visual Feedback")]
        private Renderer minionRenderer;
        private Color originalColor;

        [Header("Audio")]
        public string hitSoundName = "HitEnemy";
        public string deathSoundName = "EnemyDeath";

        public delegate void DeathDelegate();
        public event DeathDelegate OnDeath;

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
            // Validação: não pode receber dano do mesmo time
            if (attackerTeam == minionTeam && attackerTeam != Team.None)
                return;

            // Sincroniza dano via RPC
            photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damageAmount);
        }

        [PunRPC]
        private void RPC_TakeDamage(int damageAmount)
        {
            currentHealth -= damageAmount;
            StartCoroutine(FlashRed());

            // Toca som de hit
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(hitSoundName);
            }

            if (currentHealth <= 0 && PhotonNetwork.IsMasterClient)
            {
                // Apenas o MasterClient destrói o minion
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
            // Invoca evento de morte (para MinionAI descontar do limite de spawns)
            OnDeath?.Invoke();

            // Toca som de morte
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(deathSoundName);
            }

            // Destrói via Photon
            if (PhotonNetwork.IsMasterClient)
            {
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
    }
}