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

            // Ativa nova arma
            currentWeapon = weapon;
            EnableWeapon(currentWeapon);

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