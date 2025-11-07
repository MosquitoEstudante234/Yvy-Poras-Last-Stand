/*using UnityEngine;
using Photon.Pun;
using MOBAGame.Player;
using MOBAGame.Combat;
using System.Collections;

namespace MOBAGame
{
    public class RespawnManager : MonoBehaviourPun, IDamageable
    {
        [Header("Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float respawnDelay = 7f;

        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Slider healthBar;

        private float currentHealth;
        private PlayerController playerController;

        private void Start()
        {
            currentHealth = maxHealth;
            playerController = GetComponent<PlayerController>();
            UpdateHealthUI();
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            photonView.RPC("RPC_UpdateHealth", RpcTarget.All, currentHealth);

            if (currentHealth <= 0 && !playerController.IsDead)
            {
                Die();
            }
        }

        [PunRPC]
        private void RPC_UpdateHealth(float newHealth)
        {
            currentHealth = newHealth;
            UpdateHealthUI();
        }

        private void UpdateHealthUI()
        {
            if (healthBar != null)
            {
                healthBar.value = currentHealth / maxHealth;
            }
        }

        private void Die()
        {
            playerController.Die();

            if (photonView.IsMine)
            {
                StartCoroutine(RespawnCoroutine());
            }
        }

        private IEnumerator RespawnCoroutine()
        {
            yield return new WaitForSeconds(respawnDelay);

            // Obter posição de spawn da base do time
            BaseController baseCtrl = GameManager.Instance.GetBaseForTeam(playerController.PlayerTeam);
            Vector3 spawnPosition = baseCtrl.PlayerSpawnPoint.position;

            // Resetar vida
            currentHealth = maxHealth;
            photonView.RPC("RPC_UpdateHealth", RpcTarget.All, currentHealth);

            // Respawn
            playerController.Respawn(spawnPosition);
        }
    }
}
*/