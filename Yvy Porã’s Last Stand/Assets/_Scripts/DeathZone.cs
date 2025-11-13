using UnityEngine;
using Photon.Pun;
using MOBAGame.Player;

namespace MOBAGame.Hazards
{
    /// <summary>
    /// Zona de morte instantânea - mata o jogador ao contato
    /// Útil para abismos, lava, armadilhas, etc.
    /// Requer um Collider com "Is Trigger" ativado
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DeathZone : MonoBehaviour
    {
        [Header("Death Zone Settings")]
        [Tooltip("Se true, mata instantaneamente. Se false, causa dano ao longo do tempo")]
        public bool instantKill = true;

        [Tooltip("Dano por segundo (usado apenas se instantKill = false)")]
        public int damagePerSecond = 50;

        [Tooltip("Intervalo entre danos (usado apenas se instantKill = false)")]
        public float damageInterval = 0.5f;

        [Header("Visual Feedback")]
        [Tooltip("Cor opcional para visualizar a zona no editor")]
        public Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);

        [Header("Audio")]
        public string deathSoundName = "PlayerFall";

        private void Start()
        {
            // Valida se o collider está configurado como trigger
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                Debug.LogWarning("[DeathZone] Collider deve estar com 'Is Trigger' ativado!");
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Verifica se é um jogador
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                PhotonView playerPhotonView = playerHealth.GetComponent<PhotonView>();

                // Apenas processa se for o próprio jogador (evita dupla execução em multiplayer)
                if (playerPhotonView != null && playerPhotonView.IsMine)
                {
                    if (instantKill)
                    {
                        // Mata instantaneamente
                        KillPlayer(playerHealth);
                    }
                    else
                    {
                        // Inicia dano contínuo
                        StartCoroutine(ApplyContinuousDamage(playerHealth));
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Se o jogador sair da zona e o dano não for instantâneo, para o dano
            if (!instantKill)
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    StopAllCoroutines();
                }
            }
        }

        /// <summary>
        /// Mata o jogador instantaneamente
        /// </summary>
        private void KillPlayer(PlayerHealth playerHealth)
        {
            if (playerHealth.isDead) return;

            Debug.Log($"[DeathZone] {playerHealth.photonView.Owner.NickName} entrou na zona de morte!");

            // Toca som
            if (AudioManager.instance != null && !string.IsNullOrEmpty(deathSoundName))
            {
                AudioManager.instance.Play(deathSoundName);
            }

            // Aplica dano massivo para garantir morte (evita bugs de HP negativo)
            int massiveDamage = playerHealth.GetMaxHealth() + 1000;
            playerHealth.TakeDamage(massiveDamage, (PhotonView)null);
        }

        /// <summary>
        /// Aplica dano contínuo enquanto o jogador estiver na zona
        /// </summary>
        private System.Collections.IEnumerator ApplyContinuousDamage(PlayerHealth playerHealth)
        {
            Debug.Log($"[DeathZone] {playerHealth.photonView.Owner.NickName} entrou na zona de dano contínuo!");

            while (playerHealth != null && !playerHealth.isDead)
            {
                int damageAmount = Mathf.RoundToInt(damagePerSecond * damageInterval);
                playerHealth.TakeDamage(damageAmount, (PhotonView)null);

                Debug.Log($"[DeathZone] Causando {damageAmount} de dano em {playerHealth.photonView.Owner.NickName}");

                yield return new WaitForSeconds(damageInterval);
            }
        }

        /// <summary>
        /// Desenha a zona de morte no editor para facilitar visualização
        /// </summary>
        private void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = gizmoColor;

                if (col is BoxCollider boxCol)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawCube(boxCol.center, boxCol.size);
                    Gizmos.DrawWireCube(boxCol.center, boxCol.size);
                }
                else if (col is SphereCollider sphereCol)
                {
                    Gizmos.DrawSphere(transform.position + sphereCol.center, sphereCol.radius);
                    Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);
                }
                else
                {
                    // Fallback para outros tipos de collider
                    Gizmos.DrawWireCube(transform.position, Vector3.one);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Desenha em vermelho quando selecionado
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = Color.red;

                if (col is BoxCollider boxCol)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(boxCol.center, boxCol.size);
                }
                else if (col is SphereCollider sphereCol)
                {
                    Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);
                }
            }
        }
    }
}