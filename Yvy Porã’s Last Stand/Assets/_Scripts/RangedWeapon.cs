using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;
using MOBAGame.Core;
using MOBAGame.Minions;
using MOBAGame.Player;

namespace MOBAGame.Weapons
{
    public class RangedWeapon : MonoBehaviourPun
    {
        [Header("Weapon Stats")]
        public int maxAmmo = 10;
        private int currentAmmo;
        public int damage = 25;
        public float range = 50f;
        public float fireRate = 0.5f;
        private float nextFireTime = 0f;

        [Header("Reload Settings")]
        public float reloadDuration = 4f;
        private bool isReloading = false;
        private Coroutine reloadCoroutine;

        [Header("References - External Canvas")]
        public Camera fpsCam;
        public TextMeshProUGUI ammoText;
        public Slider reloadSlider;
        public GameObject muzzleFlashEffect;

        [Header("Team")]
        private Team ownerTeam = Team.None;

        [Header("Audio")]
        public string shootSoundName = "Shoot";
        public string reloadSoundName = "Reload";
        public string emptyClickSoundName = "EmptyClick";

        private void OnEnable()
        {
            // Reseta municao ao equipar
            currentAmmo = maxAmmo;
            UpdateAmmoUI();

            if (reloadSlider != null)
                reloadSlider.gameObject.SetActive(false);

            // Mostra UI de municao
            if (ammoText != null)
                ammoText.gameObject.SetActive(true);

            // Inicializa team se necessario
            if (ownerTeam == Team.None && photonView.Owner != null)
            {
                if (photonView.Owner.CustomProperties.TryGetValue("Team", out object teamValue))
                {
                    ownerTeam = (Team)((int)teamValue);
                }
            }
        }

        private void OnDisable()
        {
            // Cancela recarga ao desequipar
            CancelReload();

            // Esconde UI
            if (ammoText != null)
                ammoText.gameObject.SetActive(false);

            if (reloadSlider != null)
                reloadSlider.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!photonView.IsMine) return;
            if (!enabled) return; // Nao atira se script desabilitado

            if (Input.GetButtonDown("Fire1"))
            {
                TryShoot();
            }

            if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
            {
                StartReload();
            }
        }

        private void TryShoot()
        {
            if (isReloading)
            {
                Debug.Log("Recarregando! Nao pode atirar.");
                return;
            }

            if (Time.time < nextFireTime)
            {
                return;
            }

            if (currentAmmo <= 0)
            {
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.Play(emptyClickSoundName);
                }
                Debug.Log("Sem municao! Pressione R para recarregar.");
                return;
            }

            Shoot();
        }

        private void Shoot()
        {
            nextFireTime = Time.time + fireRate;
            currentAmmo--;
            UpdateAmmoUI();

            if (muzzleFlashEffect != null)
            {
                muzzleFlashEffect.SetActive(false);
                muzzleFlashEffect.SetActive(true);
            }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(shootSoundName);
            }

            int layerMask = ~LayerMask.GetMask("Ignore Raycast");

            if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit hit, range, layerMask))
            {
                Debug.Log($"Acertou: {hit.transform.name}");

                PlayerHealth playerHealth = hit.transform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    if (playerHealth.GetTeam() != ownerTeam && playerHealth.GetTeam() != Team.None)
                    {
                        playerHealth.photonView.RPC("TakeDamage", RpcTarget.All, damage, photonView.ViewID);
                        Debug.Log($"Causou {damage} de dano em {hit.transform.name}");
                    }
                    return;
                }

                MinionHealth minionHealth = hit.transform.GetComponent<MinionHealth>();
                if (minionHealth != null)
                {
                    if (minionHealth.GetTeam() != ownerTeam)
                    {
                        minionHealth.TakeDamage(damage, ownerTeam);
                        Debug.Log($"Causou {damage} de dano no minion {hit.transform.name}");
                    }
                    return;
                }
            }

            if (currentAmmo <= 0)
            {
                StartReload();
            }
        }

        private void StartReload()
        {
            if (isReloading) return;

            if (currentAmmo >= maxAmmo)
            {
                Debug.Log("Municao ja esta cheia!");
                return;
            }

            isReloading = true;

            if (reloadSlider != null)
            {
                reloadSlider.gameObject.SetActive(true);
                reloadSlider.maxValue = reloadDuration;
                reloadSlider.value = 0f;
            }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(reloadSoundName);
            }

            reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            float elapsed = 0f;

            while (elapsed < reloadDuration)
            {
                elapsed += Time.deltaTime;

                if (reloadSlider != null)
                {
                    reloadSlider.value = elapsed;
                }

                yield return null;
            }

            currentAmmo = maxAmmo;
            isReloading = false;
            UpdateAmmoUI();

            if (reloadSlider != null)
            {
                reloadSlider.gameObject.SetActive(false);
            }

            Debug.Log("Recarga completa!");
        }

        public void CancelReload()
        {
            if (!isReloading) return;

            isReloading = false;

            if (reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
                reloadCoroutine = null;
            }

            if (reloadSlider != null)
            {
                reloadSlider.gameObject.SetActive(false);
            }

            Debug.Log("Recarga cancelada!");
        }

        private void UpdateAmmoUI()
        {
            if (ammoText != null)
            {
                ammoText.text = $"{currentAmmo}/{maxAmmo}";
            }
        }

        public bool IsReloading()
        {
            return isReloading;
        }
    }
}