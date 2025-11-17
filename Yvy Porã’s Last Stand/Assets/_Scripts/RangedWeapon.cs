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

        [Header("Visual Effects")]
        public LineRenderer bulletTrailPrefab; // Prefab do LineRenderer para o trajeto
        public GameObject impactEffectPrefab; // Efeito de impacto (faíscas, sangue, etc)
        public float trailDuration = 0.1f; // Duração do trajeto visível

        [Header("Audio")]
        public string shootSoundName = "Shoot";
        public string reloadSoundName = "Reload";
        public string emptyClickSoundName = "EmptyClick";

        private Team ownerTeam = Team.None;
        private PlayerAnimationController animationController;

        private void OnEnable()
        {
            currentAmmo = maxAmmo;

            if (animationController == null)
            {
                animationController = GetComponentInParent<PlayerAnimationController>();
            }

            if (ownerTeam == Team.None && photonView.Owner != null)
            {
                if (photonView.Owner.CustomProperties.TryGetValue("Team", out object teamValue))
                {
                    ownerTeam = (Team)((int)teamValue);
                    Debug.Log($"[RangedWeapon] Time inicializado: {ownerTeam}");
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
            if (isReloading)
            {
                Debug.Log("[RangedWeapon] Recarregando! Nao pode atirar.");
                return;
            }

            if (Time.time < nextFireTime) return;

            if (currentAmmo <= 0)
            {
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.Play(emptyClickSoundName);
                }
                Debug.Log("[RangedWeapon] Sem municao! Pressione R para recarregar.");
                return;
            }

            Shoot();
        }

        private void Shoot()
        {
            nextFireTime = Time.time + fireRate;
            currentAmmo--;

            // Toca animação de ataque
            if (animationController != null)
            {
                animationController.PlayRangedAttack();
                Debug.Log("[RangedWeapon] Animacao de ataque ranged disparada");
            }

            if (muzzleFlashEffect != null)
            {
                muzzleFlashEffect.SetActive(false);
                muzzleFlashEffect.SetActive(true);
            }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(shootSoundName);
            }

            // Ponto de origem do tiro (posição da câmera)
            Vector3 shootOrigin = fpsCam.transform.position;
            Vector3 shootDirection = fpsCam.transform.forward;

            // Raycast para detectar alvos
            int layerMask = ~LayerMask.GetMask("Ignore Raycast");
            Vector3 hitPoint;
            bool hitSomething = false;

            if (Physics.Raycast(shootOrigin, shootDirection, out RaycastHit hit, range, layerMask))
            {
                hitPoint = hit.point;
                hitSomething = true;

                Debug.Log($"[RangedWeapon] Raycast acertou: {hit.transform.name}");

                // Tenta encontrar PlayerHealth
                PlayerHealth playerHealth = hit.transform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    if (playerHealth.GetTeam() != ownerTeam && playerHealth.GetTeam() != Team.None)
                    {
                        playerHealth.photonView.RPC("TakeDamage", RpcTarget.All, damage, photonView.ViewID);
                        Debug.Log($"[RangedWeapon] Causou {damage} de dano no jogador {hit.transform.name}");
                    }
                    else
                    {
                        Debug.Log($"[RangedWeapon] Jogador do mesmo time ignorado: {hit.transform.name}");
                    }
                }
                else
                {
                    // Tenta encontrar MinionHealth
                    MinionHealth minionHealth = hit.transform.GetComponent<MinionHealth>();

                    if (minionHealth == null)
                    {
                        minionHealth = hit.transform.GetComponentInParent<MinionHealth>();
                    }

                    if (minionHealth != null)
                    {
                        Team minionTeam = minionHealth.GetTeam();

                        Debug.Log($"[RangedWeapon] Minion detectado: {hit.transform.name}, Time: {minionTeam}, OwnerTeam: {ownerTeam}");

                        if (minionTeam != ownerTeam && minionTeam != Team.None)
                        {
                            minionHealth.TakeDamage(damage, ownerTeam);
                            Debug.Log($"[RangedWeapon] Causou {damage} de dano no minion {hit.transform.name}");
                        }
                        else
                        {
                            Debug.Log($"[RangedWeapon] Minion do mesmo time ignorado: {hit.transform.name}");
                        }
                    }
                    else
                    {
                        Debug.Log($"[RangedWeapon] Acertou objeto sem componente de dano: {hit.transform.name}");
                    }
                }

                // Sincroniza efeitos visuais via RPC
                photonView.RPC(nameof(RPC_ShowBulletTrail), RpcTarget.All, shootOrigin, hitPoint, true);
            }
            else
            {
                // Não acertou nada, trajeto vai até o alcance máximo
                hitPoint = shootOrigin + shootDirection * range;
                Debug.Log("[RangedWeapon] Raycast nao acertou nada");

                // Sincroniza efeitos visuais via RPC
                photonView.RPC(nameof(RPC_ShowBulletTrail), RpcTarget.All, shootOrigin, hitPoint, false);
            }

            // Auto-reload se ficou sem munição
            if (currentAmmo <= 0)
            {
                StartReload();
            }
        }

        /// <summary>
        /// RPC para mostrar o trajeto da bala e efeito de impacto para todos os jogadores
        /// </summary>
        [PunRPC]
        private void RPC_ShowBulletTrail(Vector3 startPoint, Vector3 endPoint, bool hitTarget)
        {
            // Desenha o trajeto da bala
            if (bulletTrailPrefab != null)
            {
                LineRenderer trail = Instantiate(bulletTrailPrefab);
                trail.SetPosition(0, startPoint);
                trail.SetPosition(1, endPoint);

                // Destrói o trajeto após alguns frames
                StartCoroutine(FadeTrail(trail));
            }

            // Instancia efeito de impacto se acertou algo
            if (hitTarget && impactEffectPrefab != null)
            {
                GameObject impact = Instantiate(impactEffectPrefab, endPoint, Quaternion.LookRotation(startPoint - endPoint));
                Destroy(impact, 2f); // Remove após 2 segundos
            }
        }

        /// <summary>
        /// Corrotina para fazer o trajeto desaparecer gradualmente
        /// </summary>
        private IEnumerator FadeTrail(LineRenderer trail)
        {
            float elapsed = 0f;
            Color startColor = trail.startColor;
            Color endColor = trail.endColor;

            while (elapsed < trailDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / trailDuration);

                trail.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
                trail.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha);

                yield return null;
            }

            Destroy(trail.gameObject);
        }

        private void StartReload()
        {
            if (isReloading) return;
            if (currentAmmo >= maxAmmo)
            {
                Debug.Log("[RangedWeapon] Municao ja esta cheia!");
                return;
            }

            isReloading = true;

            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play(reloadSoundName);
            }

            Debug.Log($"[RangedWeapon] Iniciando recarga... ({reloadDuration}s)");
            reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator ReloadCoroutine()
        {
            yield return new WaitForSeconds(reloadDuration);

            currentAmmo = maxAmmo;
            isReloading = false;

            Debug.Log("[RangedWeapon] Recarga completa!");
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

            Debug.Log("[RangedWeapon] Recarga cancelada!");
        }

        public bool IsReloading() => isReloading;
        public int GetCurrentAmmo() => currentAmmo;
        public int GetMaxAmmo() => maxAmmo;
    }
}