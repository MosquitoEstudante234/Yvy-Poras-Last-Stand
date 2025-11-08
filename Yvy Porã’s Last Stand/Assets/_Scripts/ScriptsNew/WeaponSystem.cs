using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;
using MOBAGame.Core;
using MOBAGame.Player;

namespace MOBAGame.Combat
{
    public enum WeaponType { Melee, Ranged }

    [System.Serializable]
    public class Weapon
    {
        public string weaponName;
        public WeaponType type;
        public GameObject weaponModel;
        public MonoBehaviour weaponScript; // Script da arma (MeleeWeapon ou RangedWeapon)
    }

    public class WeaponSystem : MonoBehaviourPun
    {
        [Header("Weapons")]
        [SerializeField] private Weapon meleeWeapon;
        [SerializeField] private Weapon rangedWeapon;

        [Header("Settings")]
        [SerializeField] private float weaponSwitchCooldown = 0.5f;

        [Header("UI References")]
        [SerializeField] private GameObject weaponUICanvas;

        private Weapon currentWeapon;
        private bool canSwitchWeapon = true;
        private bool isPlayerDead = false; // NOVO
        private Team playerTeam = Team.None;
        private PlayerAnimationController animationController;

        private void Awake()
        {
            // Desativa todas as armas e seus scripts no Awake
            DisableWeapon(meleeWeapon);
            DisableWeapon(rangedWeapon);
        }

        private void Start()
        {
            if (!photonView.IsMine) return;

            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
            {
                playerTeam = (Team)((int)teamValue);
            }

            animationController = GetComponent<PlayerAnimationController>();

            // Equipa arma melee como padrao
            EquipWeapon(meleeWeapon);
        }

        public WeaponType GetCurrentWeaponType()
        {
            return currentWeapon != null ? currentWeapon.type : WeaponType.Melee;
        }

        private void Update()
        {
            if (!photonView.IsMine) return;
            if (isPlayerDead) return; // NOVO: Bloqueia input se morto

            HandleWeaponSwitch();
        }

        private void HandleWeaponSwitch()
        {
            if (!canSwitchWeapon) return;

            if (Input.GetKeyDown(KeyCode.Alpha1) && currentWeapon != meleeWeapon)
            {
                StartCoroutine(SwitchWeaponCoroutine(meleeWeapon));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && currentWeapon != rangedWeapon)
            {
                StartCoroutine(SwitchWeaponCoroutine(rangedWeapon));
            }
        }

        private IEnumerator SwitchWeaponCoroutine(Weapon newWeapon)
        {
            canSwitchWeapon = false;

            // Cancela recarga se estiver mudando de ranged
            if (currentWeapon == rangedWeapon && currentWeapon.weaponScript != null)
            {
                MOBAGame.Weapons.RangedWeapon rangedScript = currentWeapon.weaponScript as MOBAGame.Weapons.RangedWeapon;
                if (rangedScript != null)
                {
                    rangedScript.CancelReload();
                }
            }

            EquipWeapon(newWeapon);

            yield return new WaitForSeconds(weaponSwitchCooldown);
            canSwitchWeapon = true;
        }

        private void EquipWeapon(Weapon weapon)
        {
            // Desativa arma anterior
            if (currentWeapon != null)
            {
                DisableWeapon(currentWeapon);
            }

            // Ativa nova arma (apenas se jogador não estiver morto)
            currentWeapon = weapon;

            if (!isPlayerDead)
            {
                EnableWeapon(currentWeapon);
            }

            // Sincroniza com outros jogadores
            photonView.RPC("RPC_EquipWeapon", RpcTarget.Others, weapon == meleeWeapon);

            Debug.Log($"[WeaponSystem] Equipou: {currentWeapon.weaponName}");
        }

        private void EnableWeapon(Weapon weapon)
        {
            if (weapon.weaponModel != null)
                weapon.weaponModel.SetActive(true);

            if (weapon.weaponScript != null)
                weapon.weaponScript.enabled = true;
        }

        private void DisableWeapon(Weapon weapon)
        {
            if (weapon.weaponModel != null)
                weapon.weaponModel.SetActive(false);

            if (weapon.weaponScript != null)
                weapon.weaponScript.enabled = false;
        }

        /// <summary>
        /// Desativa todas as armas quando o jogador morre
        /// Chamado pelo PlayerHealth
        /// </summary>
        public void DisableAllWeapons()
        {
            isPlayerDead = true;

            if (currentWeapon != null)
            {
                DisableWeapon(currentWeapon);
            }

            // Garante que ambas estão desativadas
            DisableWeapon(meleeWeapon);
            DisableWeapon(rangedWeapon);

            Debug.Log("[WeaponSystem] Todas as armas desativadas (jogador morto)");
        }

        /// <summary>
        /// Reativa a arma atual quando o jogador respawna
        /// Chamado pelo PlayerHealth
        /// </summary>
        public void EnableCurrentWeapon()
        {
            isPlayerDead = false;

            if (currentWeapon != null)
            {
                EnableWeapon(currentWeapon);
                Debug.Log($"[WeaponSystem] Arma reativada: {currentWeapon.weaponName}");
            }
            else
            {
                // Se não houver arma equipada, equipa a melee por padrão
                EquipWeapon(meleeWeapon);
                Debug.Log("[WeaponSystem] Equipando arma padrão (Melee) após respawn");
            }
        }

        [PunRPC]
        private void RPC_EquipWeapon(bool isMelee)
        {
            if (meleeWeapon.weaponModel != null)
                meleeWeapon.weaponModel.SetActive(isMelee);
            if (rangedWeapon.weaponModel != null)
                rangedWeapon.weaponModel.SetActive(!isMelee);
        }
    }
}