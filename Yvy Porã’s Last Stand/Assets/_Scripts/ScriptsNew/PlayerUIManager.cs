using UnityEngine;
using TMPro;
using Photon.Pun;
using MOBAGame.Player;
using MOBAGame.Weapons;

namespace MOBAGame.UI
{
    /// <summary>
    /// Gerencia a UI de vida e munição do jogador local
    /// Canvas independente na hierarquia, não filho do player
    /// </summary>
    public class PlayerUIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI ammoText;

        [Header("Update Settings")]
        [SerializeField] private float updateInterval = 0.1f; // Atualiza 10x por segundo

        private PlayerHealth localPlayerHealth;
        private RangedWeapon currentRangedWeapon;
        private float nextUpdateTime = 0f;

        private void Start()
        {
            FindLocalPlayer();
        }

        private void Update()
        {
            // Tenta encontrar o jogador se ainda não encontrou
            if (localPlayerHealth == null)
            {
                FindLocalPlayer();
                return;
            }

            // Atualiza UI em intervalos (performance)
            if (Time.time >= nextUpdateTime)
            {
                UpdateUI();
                nextUpdateTime = Time.time + updateInterval;
            }
        }

        /// <summary>
        /// Busca o jogador local na cena (com photonView.IsMine)
        /// </summary>
        private void FindLocalPlayer()
        {
            PlayerHealth[] allPlayers = FindObjectsOfType<PlayerHealth>();

            foreach (PlayerHealth player in allPlayers)
            {
                PhotonView photonView = player.GetComponent<PhotonView>();

                if (photonView != null && photonView.IsMine)
                {
                    localPlayerHealth = player;
                    Debug.Log("[PlayerUIManager] Jogador local encontrado!");

                    // Busca a arma ranged (se tiver)
                    currentRangedWeapon = player.GetComponentInChildren<RangedWeapon>(true);

                    UpdateUI();
                    return;
                }
            }
        }

        /// <summary>
        /// Atualiza os textos de vida e munição
        /// </summary>
        private void UpdateUI()
        {
            if (localPlayerHealth == null) return;

            // Atualiza vida
            if (healthText != null)
            {
                int currentHealth = localPlayerHealth.GetCurrentHealth();
                int maxHealth = localPlayerHealth.GetMaxHealth();
                healthText.text = $"Vida: {currentHealth}/{maxHealth}";
            }

            // Atualiza munição (apenas se tiver arma ranged equipada)
            if (ammoText != null)
            {
                // Re-busca a arma ranged (caso tenha trocado de arma)
                RangedWeapon rangedWeapon = localPlayerHealth.GetComponentInChildren<RangedWeapon>(true);

                if (rangedWeapon != null && rangedWeapon.enabled)
                {
                    // Acessa valores privados através de reflexão (temporário)
                    var currentAmmoField = typeof(RangedWeapon).GetField("currentAmmo",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (currentAmmoField != null)
                    {
                        int currentAmmo = (int)currentAmmoField.GetValue(rangedWeapon);
                        int maxAmmo = rangedWeapon.maxAmmo;
                        ammoText.text = $"Munição: {currentAmmo}/{maxAmmo}";
                    }
                }
                else
                {
                    // Se não tem arma ranged equipada, esconde texto de munição
                    ammoText.text = "";
                }
            }
        }

        /// <summary>
        /// Método público para forçar atualização (opcional)
        /// </summary>
        public void ForceUpdate()
        {
            UpdateUI();
        }
    }
}