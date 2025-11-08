using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;
using MOBAGame.Core;
using MOBAGame.Minions;
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
        public float damage;
        public float range;
        public float attackCooldown;
        public int maxAmmo;
        public ParticleSystem muzzleFlash;
    }

    public class WeaponSystem : MonoBehaviourPun
    {
        [Header("Weapons")]
        [SerializeField] private Weapon meleeWeapon;
        [SerializeField] private Weapon rangedWeapon;

        [Header("Settings")]
        [SerializeField] private float weaponSwitchCooldown = 0.5f;
        [SerializeField] private float reloadDuration = 4f;
        [SerializeField] private LayerMask damageableLayers;
        [SerializeField] private Transform attackPoint;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private TextMeshProUGUI reloadText;

        private Weapon currentWeapon;
        private int currentAmmo;
        private bool isReloading = false;
        private bool canSwitchWeapon = true;
        private float lastAttackTime = 0f;
        private Team playerTeam = Team.None;
        private PlayerAnimationController animationController;

        private void Awake()
        {
            // Desativa todas as armas no Awake (antes de qualquer logica)
            if (meleeWeapon.weaponModel != null)
                meleeWeapon.weaponModel.SetActive(false);
            if (rangedWeapon.weaponModel != null)
                rangedWeapon.weaponModel.SetActive(false);
        }

        private void Start()
        {
            if (!photonView.IsMine) return;

            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
            {
                playerTeam = (Team)((int)teamValue);
            }

            animationController = GetComponent<PlayerAnimationController>();

            // Equipa arma inicial (melee)
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
            HandleAttack();
            HandleReload();
            UpdateUI();
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

            if (isReloading)
            {
                StopCoroutine("ReloadCoroutine");
                isReloading = false;

                if (reloadText != null)
                    reloadText.gameObject.SetActive(false);
            }

            EquipWeapon(newWeapon);

            yield return new WaitForSeconds(weaponSwitchCooldown);
            canSwitchWeapon = true;
        }

        private void EquipWeapon(Weapon weapon)
        {
            // Desativar todas as armas
            if (meleeWeapon.weaponModel != null)
                meleeWeapon.weaponModel.SetActive(false);
            if (rangedWeapon.weaponModel != null)
                rangedWeapon.weaponModel.SetActive(false);

            // Ativar arma atual
            currentWeapon = weapon;
            if (currentWeapon.weaponModel != null)
                currentWeapon.weaponModel.SetActive(true);

            // Resetar municao se for ranged
            if (currentWeapon.type == WeaponType.Ranged)
            {
                currentAmmo = currentWeapon.maxAmmo;
            }

            // Sincroniza com outros jogadores
            photonView.RPC("RPC_EquipWeapon", RpcTarget.Others, weapon == meleeWeapon);
        }

        [PunRPC]
        private void RPC_EquipWeapon(bool isMelee)
        {
            if (meleeWeapon.weaponModel != null)
                meleeWeapon.weaponModel.SetActive(isMelee);
            if (rangedWeapon.weaponModel != null)
                rangedWeapon.weaponModel.SetActive(!isMelee);
        }

        private void HandleAttack()
        {
            if (Time.time < lastAttackTime + currentWeapon.attackCooldown)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (currentWeapon.type == WeaponType.Ranged && currentAmmo <= 0)
                {
                    if (reloadText != null)
                    {
                        reloadText.text = "Pressione R para recarregar!";
                        reloadText.gameObject.SetActive(true);
                    }
                    return;
                }

                Attack();
                lastAttackTime = Time.time;
            }
        }

        private void Attack()
        {
            if (animationController != null)
            {
                if (currentWeapon.type == WeaponType.Melee)
                {
                    animationController.PlayMeleeAttack();
                }
                else
                {
                    animationController.PlayRangedAttack();
                }
            }

            if (attackPoint == null)
            {
                Debug.LogWarning("WeaponSystem: AttackPoint nao esta configurado!");
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(attackPoint.position, attackPoint.forward, out hit, currentWeapon.range, damageableLayers))
            {
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    if (playerHealth.GetTeam() != playerTeam && playerHealth.GetTeam() != Team.None)
                    {
                        playerHealth.TakeDamage((int)currentWeapon.damage, photonView);
                        Debug.Log($"[WeaponSystem] Causou {currentWeapon.damage} de dano em jogador {hit.collider.name}");
                    }
                    else
                    {
                        Debug.Log("[WeaponSystem] Nao pode atacar o proprio time!");
                    }
                }

                MinionHealth minionHealth = hit.collider.GetComponent<MinionHealth>();
                if (minionHealth != null)
                {
                    if (minionHealth.GetTeam() != playerTeam)
                    {
                        minionHealth.TakeDamage((int)currentWeapon.damage, playerTeam);
                        Debug.Log($"[WeaponSystem] Causou {currentWeapon.damage} de dano no minion {hit.collider.name}");
                    }
                }
            }

            if (currentWeapon.type == WeaponType.Ranged)
            {
                currentAmmo--;

                if (currentWeapon.muzzleFlash != null)
                    currentWeapon.muzzleFlash.Play();
            }
        }

        private void HandleReload()
        {
            if (currentWeapon.type != WeaponType.Ranged) return;
            if (isReloading) return;
            if (currentAmmo >= currentWeapon.maxAmmo) return;

            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(ReloadCoroutine());
            }
        }

        private IEnumerator ReloadCoroutine()
        {
            isReloading = true;

            if (reloadText != null)
            {
                reloadText.text = "Recarregando...";
                reloadText.gameObject.SetActive(true);
            }

            float elapsedTime = 0f;
            while (elapsedTime < reloadDuration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;

                if (currentWeapon != rangedWeapon)
                {
                    isReloading = false;
                    if (reloadText != null)
                        reloadText.gameObject.SetActive(false);
                    yield break;
                }
            }

            currentAmmo = currentWeapon.maxAmmo;
            isReloading = false;

            if (reloadText != null)
                reloadText.gameObject.SetActive(false);
        }

        private void UpdateUI()
        {
            if (currentWeapon.type == WeaponType.Ranged)
            {
                if (ammoText != null)
                {
                    ammoText.text = $"{currentAmmo} / {currentWeapon.maxAmmo}";
                    ammoText.gameObject.SetActive(true);
                }
            }
            else
            {
                if (ammoText != null)
                    ammoText.gameObject.SetActive(false);
            }

            if (cooldownFillImage != null)
            {
                float cooldownProgress = Mathf.Clamp01((Time.time - lastAttackTime) / currentWeapon.attackCooldown);
                cooldownFillImage.fillAmount = cooldownProgress;
            }
        }
    }
}