using UnityEngine;
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

        [Header("References")]
        public Camera fpsCam;
        public GameObject muzzleFlashEffect;

        // REMOVIDO: ammoText, reloadSlider

        [Header("Audio")]
        public string shootSoundName = "Shoot";
        public string reloadSoundName = "Reload";
        public string emptyClickSoundName = "EmptyClick";

        private Team ownerTeam = Team.None;

        private void OnEnable()
        {
            currentAmmo = maxAmmo;

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
            CancelReload();
        }

        private void Update()
        {
            if (!photonView.IsMine) return;
            if (!enabled) return;

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
            if (isReloading) return;
            if (Time.time < nextFireTime) return;

            if (currentAmmo <= 0)
            {
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.Play(emptyClickSoundName);
                }
                return;
            }

            Shoot();
        }

        private void Shoot()
        {
            nextFireTime = Time.time + fireRate;
            currentAmmo--;

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
                PlayerHealth playerHealth = hit.transform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    if (playerHealth.GetTeam() != ownerTeam && playerHealth.GetTeam() != Team.None)
                    {
                        playerHealth.photonView.RPC("TakeDamage", RpcTarget.All, damage, photonView.ViewID);
                    }
                    return;
                }

                MinionHealth minionHealth = hit.transform.GetComponent<MinionHealth>();
                if (minionHealth != null)
                {
                    if (minionHealth.GetTeam() != ownerTeam)
                    {
                        minionHealth.TakeDamage(damage, ownerTeam);
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
            if (currentAmmo >= maxAmmo) return;

            isReloading = true;

            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(reloadSoundName);
            }

            reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            yield return new WaitForSeconds(reloadDuration);

            currentAmmo = maxAmmo;
            isReloading = false;
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
        }

        // REMOVIDO: UpdateAmmoUI()

        public bool IsReloading() => isReloading;
        public int GetCurrentAmmo() => currentAmmo;
        public int GetMaxAmmo() => maxAmmo;
    }
}