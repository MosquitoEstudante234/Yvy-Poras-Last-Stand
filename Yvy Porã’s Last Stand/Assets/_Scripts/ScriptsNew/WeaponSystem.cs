using UnityEngine;
using UnityEngine.UI;
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
        public int maxAmmo; // Apenas para ranged
        public ParticleSystem muzzleFlash; // Apenas para ranged
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
        [SerializeField] private Text ammoText;
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private Text reloadText;

        private Weapon currentWeapon;
        private int currentAmmo;
        private bool isReloading = false;
        private bool canSwitchWeapon = true;
        private float lastAttackTime = 0f;
        private Team playerTeam = Team.None;

        private PlayerAnimationController animationController;

        private void Start()
        {
            if (!photonView.IsMine) return;

            // Obtém o time do jogador
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
            {
                playerTeam = (Team)((int)teamValue);
            }

            EquipWeapon(meleeWeapon);

            if (!photonView.IsMine) return;

            animationController = GetComponent<PlayerAnimationController>();
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

            // Se estiver recarregando, cancelar recarga
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

            // Resetar munição se for ranged
            if (currentWeapon.type == WeaponType.Ranged)
            {
                currentAmmo = currentWeapon.maxAmmo;
            }

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
            // Verificar cooldown de ataque (persistente entre trocas)
            if (Time.time < lastAttackTime + currentWeapon.attackCooldown)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                // Verificar se precisa recarregar
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
            // Animação de ataque
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(currentWeapon.type == WeaponType.Melee ? "AttackMelee" : "AttackRanged");
            }

            // Raycast para detecção de hit
            if (attackPoint == null)
            {
                Debug.LogWarning("AttackPoint não está configurado!");
                return;
            }
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

            RaycastHit hit;
            if (Physics.Raycast(attackPoint.position, attackPoint.forward, out hit, currentWeapon.range, damageableLayers))
            {
                // Verifica se acertou um jogador
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Valida friendly fire
                    if (playerHealth.GetTeam() != playerTeam && playerHealth.GetTeam() != Team.None)
                    {
                        playerHealth.TakeDamage((int)currentWeapon.damage, photonView);
                        Debug.Log($"Causou {currentWeapon.damage} de dano em {hit.collider.name}");
                    }
                    else
                    {
                        Debug.Log("Não pode atacar o próprio time!");
                    }
                }

                // Verifica se acertou um minion
                MinionHealth minionHealth = hit.collider.GetComponent<MinionHealth>();
                if (minionHealth != null)
                {
                    if (minionHealth.GetTeam() != playerTeam)
                    {
                        minionHealth.TakeDamage((int)currentWeapon.damage, playerTeam);
                        Debug.Log($"Causou {currentWeapon.damage} de dano no minion {hit.collider.name}");
                    }
                }
            }

            // Consumir munição se for ranged
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

                // Se trocar de arma, cancelar recarga
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

            // Atualizar barra de cooldown
            if (cooldownFillImage != null)
            {
                float cooldownProgress = Mathf.Clamp01((Time.time - lastAttackTime) / currentWeapon.attackCooldown);
                cooldownFillImage.fillAmount = cooldownProgress;
            }
        }
    }
}