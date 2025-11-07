using UnityEngine;
using Photon.Pun;
using MOBAGame.Lobby;
using MOBAGame.Combat;
using UnityEngine.UI;
using MOBAGame.Core;

namespace MOBAGame
{
    public class BaseController : MonoBehaviourPun, IDamageable
    {
        [Header("Base Settings")]
        [SerializeField] public Team baseTeam;
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform minionSpawnPoint;

        [Header("UI")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Text healthText;

        private float currentHealth;

        public Team BaseTeam => baseTeam;
        public Transform PlayerSpawnPoint => playerSpawnPoint;
        public Transform MinionSpawnPoint => minionSpawnPoint;

        private void Start()
        {
            currentHealth = maxHealth;
            UpdateUI();
        }

        public void TakeDamage(float damage)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            photonView.RPC("RPC_UpdateHealth", RpcTarget.All, currentHealth);

            if (currentHealth <= 0)
            {
                BaseDestroyed();
            }
        }

        [PunRPC]
        private void RPC_UpdateHealth(float newHealth)
        {
            currentHealth = newHealth;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (healthBar != null)
            {
                healthBar.value = currentHealth / maxHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0} / {maxHealth}";
            }
        }

        private void BaseDestroyed()
        {
            photonView.RPC("RPC_BaseDestroyed", RpcTarget.All, (int)baseTeam);
        }

        [PunRPC]
        private void RPC_BaseDestroyed(int destroyedTeam)
        {
            Team winner = (Team)destroyedTeam == Team.Indigenous ? Team.Portuguese : Team.Indigenous;
            GameManager.Instance?.EndGame(winner);
        }
    }
}