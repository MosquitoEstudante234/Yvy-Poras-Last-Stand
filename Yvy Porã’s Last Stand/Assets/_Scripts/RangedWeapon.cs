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
        public float fireRate = 0.5f; // Tempo entre disparos
        private float nextFireTime = 0f;

        [Header("Reload Settings")]
        public float reloadDuration = 4f;
        private bool isReloading = false;
        private Coroutine reloadCoroutine;

        [Header("References")]
        public Camera fpsCam;
        public TextMeshProUGUI ammoText;
        public Slider reloadSlider;
        public GameObject muzzleFlashEffect; // Opcional: efeito visual de disparo

        [Header("Team")]
        private Team ownerTeam = Team.None;

        [Header("Audio")]
        public string shootSoundName = "Shoot";
        public string reloadSoundName = "Reload";
        public string emptyClickSoundName = "EmptyClick";

        private void Start()
        {
            currentAmmo = maxAmmo;
            UpdateAmmoUI();

            if (reloadSlider != null)
            {
                reloadSlider.gameObject.SetActive(false);
                reloadSlider.maxValue = reloadDuration;
                reloadSlider.value = 0f;
            }

            // Obtém o time do dono da arma
            if (photonView.Owner != null && photonView.Owner.CustomProperties.TryGetValue("Team", out object teamValue))
            {
                ownerTeam = (Team)((int)teamValue);
            }
        }

        private void Update()
        {
            // Apenas o dono da arma pode atirar
            if (!photonView.IsMine) return;

            // Detecta disparo
            if (Input.GetButtonDown("Fire1"))
            {
                TryShoot();
            }

            // Detecta recarga manual (Tecla R)
            if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
            {
                StartReload();
            }
        }

        /// <summary>
        /// Tenta disparar a arma
        /// </summary>
        private void TryShoot()
        {
            // Validações
            if (isReloading)
            {
                Debug.Log("Recarregando! Não pode atirar.");
                return;
            }

            if (Time.time < nextFireTime)
            {
                Debug.Log("Cooldown ativo!");
                return;
            }

            if (currentAmmo <= 0)
            {
                // Munição vazia - toca som de "click"
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.Play(emptyClickSoundName);
                }
                Debug.Log("Sem munição! Pressione R para recarregar.");
                return;
            }

            // Executa disparo
            Shoot();
        }

        /// <summary>
        /// Dispara a arma (raycast + dano)
        /// </summary>
        private void Shoot()
        {
            nextFireTime = Time.time + fireRate;
            currentAmmo--;
            UpdateAmmoUI();

            // Efeito de muzzle flash (opcional)
            if (muzzleFlashEffect != null)
            {
                muzzleFlashEffect.SetActive(false);
                muzzleFlashEffect.SetActive(true);
            }

            // Som de disparo
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(shootSoundName);
            }

            // Raycast para detectar acerto
            int layerMask = ~LayerMask.GetMask("Ignore Raycast");

            if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit hit, range, layerMask))
            {
                Debug.Log($"Acertou: {hit.transform.name}");

                // Verifica se acertou um jogador inimigo
                PlayerHealth playerHealth = hit.transform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Valida se é inimigo (não pode atirar no mesmo time)
                    if (playerHealth.GetTeam() != ownerTeam && playerHealth.GetTeam() != Team.None)
                    {
                        // Envia dano via RPC
                        playerHealth.photonView.RPC("TakeDamage", RpcTarget.All, damage, photonView.ViewID);
                        Debug.Log($"Causou {damage} de dano em {hit.transform.name}");
                    }
                    else
                    {
                        Debug.Log("Não pode atirar no próprio time!");
                    }
                    return;
                }

                // Verifica se acertou um minion inimigo
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

                // Verifica se acertou uma base inimiga (não causa dano, bases só recebem dano de minions)
                BaseController baseController = hit.transform.GetComponent<BaseController>();
                if (baseController != null)
                {
                    Debug.Log("Jogadores não podem atacar bases diretamente!");
                }
            }

            // Se munição acabou, inicia recarga automática
            if (currentAmmo <= 0)
            {
                StartReload();
            }
        }

        /// <summary>
        /// Inicia o processo de recarga
        /// </summary>
        private void StartReload()
        {
            if (isReloading) return;

            // Cancela se já estiver com munição cheia
            if (currentAmmo >= maxAmmo)
            {
                Debug.Log("Munição já está cheia!");
                return;
            }

            isReloading = true;

            if (reloadSlider != null)
            {
                reloadSlider.gameObject.SetActive(true);
                reloadSlider.value = 0f;
            }

            // Som de recarga
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(reloadSoundName);
            }

            reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }

        /// <summary>
        /// Corrotina de recarga com barra de progresso
        /// </summary>
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

            // Recarga completa
            currentAmmo = maxAmmo;
            isReloading = false;
            UpdateAmmoUI();

            if (reloadSlider != null)
            {
                reloadSlider.gameObject.SetActive(false);
            }

            Debug.Log("Recarga completa!");
        }

        /// <summary>
        /// Cancela a recarga (chamado pelo WeaponSystem ao trocar de arma)
        /// </summary>
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

            Debug.Log("Recarga cancelada ao trocar de arma!");
        }

        /// <summary>
        /// Atualiza a UI de munição
        /// </summary>
        private void UpdateAmmoUI()
        {
            if (ammoText != null)
            {
                ammoText.text = $"{currentAmmo}/{maxAmmo}";
            }
        }

        /// <summary>
        /// Getter para verificar se está recarregando
        /// </summary>
        public bool IsReloading()
        {
            return isReloading;
        }

        /// <summary>
        /// Ativa/desativa a arma (chamado pelo WeaponSystem)
        /// </summary>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);

            // Se desativou durante recarga, cancela
            if (!active && isReloading)
            {
                CancelReload();
            }
        }
    }
}