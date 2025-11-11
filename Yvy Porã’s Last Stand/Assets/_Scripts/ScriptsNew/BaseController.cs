using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using MOBAGame.Core;
using DG.Tweening; // Adicionado: para animações com DOTween

namespace MOBAGame
{
    public class BaseController : MonoBehaviourPun
    {
        [Header("Base Settings")]
        public Team baseTeam;
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform minionSpawnPoint;

        [Header("UI")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText;

        private float currentHealth;

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

        public void TakeDamage(int damage)
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

                // Animação de feedback ao atualizar a barra
                healthBar.transform.DOKill();
                healthBar.transform.localScale = Vector3.one;
                healthBar.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 8, 1f)
                    .SetEase(Ease.OutQuad);
            }

            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";

                // Animação opcional para o texto
                healthText.transform.DOKill();
                healthText.transform.localScale = Vector3.one;
                healthText.transform.DOPunchScale(Vector3.one * 0.1f, 0.25f, 8, 1f)
                    .SetEase(Ease.OutQuad);
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

            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame(winner);
            }
            else
            {
                Debug.LogError("BaseController: GameManager.Instance é nulo!");
            }

            GetComponent<Renderer>()?.material.SetColor("_Color", Color.red);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = baseTeam == Team.Indigenous ? Color.green : Color.blue;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 3f);

            if (playerSpawnPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(playerSpawnPoint.position, 1f);
                Gizmos.DrawLine(transform.position, playerSpawnPoint.position);
            }

            if (minionSpawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(minionSpawnPoint.position, 1f);
                Gizmos.DrawLine(transform.position, minionSpawnPoint.position);
            }
        }
    }
}
