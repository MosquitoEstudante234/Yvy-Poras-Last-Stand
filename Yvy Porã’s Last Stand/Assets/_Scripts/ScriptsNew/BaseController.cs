using UnityEngine;
using Photon.Pun;
using TMPro; // ADICIONADO: TextMeshPro
using UnityEngine.UI;
using MOBAGame.Core;

namespace MOBAGame
{
    public class BaseController : MonoBehaviourPun
    {
        [Header("Base Settings")]
        public Team baseTeam; // Público para acesso direto
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform minionSpawnPoint;

        [Header("UI")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText; // ALTERADO

        private float currentHealth;

        // Properties públicas para acesso externo
        public Team BaseTeam => baseTeam;
        public Transform PlayerSpawnPoint => playerSpawnPoint;
        public Transform MinionSpawnPoint => minionSpawnPoint;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Start()
        {
            currentHealth = maxHealth;
            UpdateUI();
        }

        /// <summary>
        /// Recebe dano na base (apenas MasterClient processa)
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            // Sincroniza vida com todos os clientes
            photonView.RPC("RPC_UpdateHealth", RpcTarget.All, currentHealth);

            if (currentHealth <= 0)
            {
                BaseDestroyed();
            }
        }

        /// <summary>
        /// RPC para sincronizar a vida da base entre todos os clientes
        /// </summary>
        [PunRPC]
        private void RPC_UpdateHealth(float newHealth)
        {
            currentHealth = newHealth;
            UpdateUI();
        }

        /// <summary>
        /// Atualiza a UI da barra de vida
        /// </summary>
        private void UpdateUI()
        {
            if (healthBar != null)
            {
                healthBar.value = currentHealth / maxHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
            }
        }

        /// <summary>
        /// Chamado quando a base é destruída
        /// </summary>
        private void BaseDestroyed()
        {
            photonView.RPC("RPC_BaseDestroyed", RpcTarget.All, (int)baseTeam);
        }

        /// <summary>
        /// RPC que finaliza o jogo quando a base é destruída
        /// </summary>
        [PunRPC]
        private void RPC_BaseDestroyed(int destroyedTeam)
        {
            Team winner = (Team)destroyedTeam == Team.Indigenous ? Team.Portuguese : Team.Indigenous;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame(winner);
            }
            else
            {
                Debug.LogError("BaseController: GameManager.Instance é nulo!");
            }

            // Opcional: Desabilita a base visualmente
            GetComponent<Renderer>()?.material.SetColor("_Color", Color.red);
        }

        /// <summary>
        /// Debug visual dos spawn points no Editor
        /// </summary>
        private void OnDrawGizmos()
        {
            // Desenha gizmo da base
            Gizmos.color = baseTeam == Team.Indigenous ? Color.green : Color.blue;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 3f);

            // Desenha spawn point de jogador
            if (playerSpawnPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(playerSpawnPoint.position, 1f);
                Gizmos.DrawLine(transform.position, playerSpawnPoint.position);
            }

            // Desenha spawn point de minions
            if (minionSpawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(minionSpawnPoint.position, 1f);
                Gizmos.DrawLine(transform.position, minionSpawnPoint.position);
            }
        }
    }
}