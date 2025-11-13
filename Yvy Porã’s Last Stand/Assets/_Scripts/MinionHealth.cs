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
        [SerializeField] private float deathAnimationDuration = 2f; // NOVO

        [Header("Visual Feedback")]
        private Renderer minionRenderer;
        private Color originalColor;

        [Header("Audio")]
        public string hitSoundName = "HitEnemy";
        public string deathSoundName = "EnemyDeath";

        public delegate void DeathDelegate();
        public event DeathDelegate OnDeath;

        private bool isDead = false; // NOVO

        private void Start()
        {
            currentHealth = maxHealth;

            minionRenderer = GetComponent<Renderer>();
            if (minionRenderer != null)
            {
                minionRenderer.material = new Material(minionRenderer.material);
                originalColor = minionRenderer.material.color;
                SetTeamColor();
            }
        }

        private void SetTeamColor()
        {
            if (minionRenderer == null) return;

            switch (minionTeam)
            {
                case Team.Indigenous:
                    minionRenderer.material.color = new Color(0.4f, 0.6f, 0.3f);
                    break;
                case Team.Portuguese:
                    minionRenderer.material.color = new Color(0.3f, 0.4f, 0.6f);
                    break;
            }

            originalColor = minionRenderer.material.color;
        }

        public void TakeDamage(int damageAmount, Team attackerTeam)
        {
            if (isDead) return; // NOVO: Previne dano após morte

            if (attackerTeam == minionTeam && attackerTeam != Team.None)
                return;

            photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damageAmount);
        }

        [PunRPC]
        private void RPC_TakeDamage(int damageAmount)
        {
            if (isDead) return; // NOVO: Previne dano após morte

            currentHealth -= damageAmount;
            StartCoroutine(FlashRed());

            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(hitSoundName);
            }

            if (currentHealth <= 0 && PhotonNetwork.IsMasterClient)
            {
                Die();
            }
        }

        private IEnumerator FlashRed()
        {
            if (minionRenderer != null)
            {
                minionRenderer.material.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                minionRenderer.material.color = originalColor;
            }
        }

        // MODIFICADO: Agora aguarda a animação antes de destruir
        private void Die()
        {
            if (isDead) return; // NOVO: Previne múltiplas chamadas
            isDead = true;

            // Invoca evento de morte (para MinionAI descontar do limite de spawns)
            OnDeath?.Invoke();

            // Toca som de morte
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(deathSoundName);
            }

            // NOVO: Aguarda animação antes de destruir
            StartCoroutine(DestroyAfterAnimation());
        }

        // NOVO: Coroutine para aguardar animação
        private IEnumerator DestroyAfterAnimation()
        {
            Debug.Log($"[MinionHealth] Aguardando {deathAnimationDuration}s para destruir minion");

            yield return new WaitForSeconds(deathAnimationDuration);

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("[MinionHealth] Destruindo minion via PhotonNetwork.Destroy");
                PhotonNetwork.Destroy(gameObject);
            }
        }

        public void AttackTarget(GameObject target)
        {
            if (Time.time < lastAttackTime + attackCooldown)
                return;

            lastAttackTime = Time.time;

            BaseController baseController = target.GetComponent<BaseController>();
            if (baseController != null)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    baseController.TakeDamage(damage);
                }
                return;
            }

            MinionHealth enemyMinion = target.GetComponent<MinionHealth>();
            if (enemyMinion != null)
            {
                if (enemyMinion.minionTeam != this.minionTeam)
                {
                    enemyMinion.TakeDamage(damage, this.minionTeam);
                }
                return;
            }

            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (playerHealth.GetTeam() != this.minionTeam)
                {
                    playerHealth.TakeDamage(damage, this.photonView);
                }
            }
        }

        public Team GetTeam()
        {
            return minionTeam;
        }

        public void SetTeam(Team team)
        {
            minionTeam = team;
            SetTeamColor();
        }

        // NOVO: Getter para verificar se está morto
        public bool IsDead() => isDead;
    }
}