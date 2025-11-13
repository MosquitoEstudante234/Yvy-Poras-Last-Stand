using MOBAGame.Combat;
using MOBAGame.Core;
using Photon.Pun;
using UnityEngine;

namespace MOBAGame.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviourPun
    {
        public Animator animator;
        public PlayerController playerController;
        public CharacterController characterController;
        private PlayerHealth playerHealth;
        private WeaponSystem weaponSystem;

        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsJumping = Animator.StringToHash("IsJumping");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        private static readonly int AttackMelee = Animator.StringToHash("AttackMelee");
        private static readonly int AttackRanged = Animator.StringToHash("AttackRanged");
        private static readonly int WeaponType = Animator.StringToHash("WeaponType");
        private static readonly int Speed = Animator.StringToHash("Speed");

        [SerializeField] private float speedSmoothTime = 0.08f;
        private float currentSpeed;

        [Header("Network Sync")]
        [SerializeField] private float syncInterval = 0.1f;
        [SerializeField] private float speedThreshold = 0.1f;
        private float lastSyncTime = 0f;
        private float lastSyncedSpeed = 0f;

        [Header("Debug")]
        [Tooltip("Ativa logs detalhados para debug")]
        public bool debugLogs = false;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator == null)
            {
                Debug.LogError("PlayerAnimationController: Animator nao encontrado no Player ou filhos!");
                enabled = false;
                return;
            }

            characterController = GetComponent<CharacterController>();
            playerController = GetComponent<PlayerController>();
            playerHealth = GetComponent<PlayerHealth>();
            weaponSystem = GetComponent<WeaponSystem>();

            Debug.Log("PlayerAnimationController: Inicializado com sucesso");
        }

        private void Update()
        {
            UpdateMovementAnimations();
            UpdateWeaponAnimations();
        }

        private void UpdateMovementAnimations()
        {
            if (playerController == null || playerHealth == null) return;

            // PRIORIDADE MÁXIMA: Se morto, força todos os parâmetros e retorna
            if (playerHealth.isDead)
            {
                if (photonView.IsMine)
                {
                    SetSpeed(0f);
                    animator.SetBool(IsJumping, false); // NOVO: Força IsJumping = false
                    animator.SetBool(IsRunning, false);
                }
                animator.SetBool(IsDead, true);
                return; // Retorna imediatamente, não processa mais nada
            }
            else
            {
                animator.SetBool(IsDead, false);
            }

            if (photonView.IsMine)
            {
                Vector3 worldVelocity = Vector3.zero;
                if (characterController != null)
                    worldVelocity = characterController.velocity;

                Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

                float speedA = new Vector2(localVelocity.x, localVelocity.z).magnitude;
                float speedB = Mathf.Abs(localVelocity.z);
                Vector3 worldHorizontal = new Vector3(worldVelocity.x, 0f, worldVelocity.z);
                float speedC = worldHorizontal.magnitude;

                float chosenSpeed = Mathf.Max(speedA, speedB, speedC);

                currentSpeed = Mathf.Lerp(currentSpeed, chosenSpeed, 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.0001f, speedSmoothTime)));

                animator.SetFloat(Speed, currentSpeed);
                animator.SetBool(IsRunning, currentSpeed > 0.1f);

                bool isGrounded = playerController.IsGrounded();
                animator.SetBool(IsJumping, !isGrounded);

                if (Time.time - lastSyncTime >= syncInterval)
                {
                    if (Mathf.Abs(currentSpeed - lastSyncedSpeed) > speedThreshold)
                    {
                        photonView.RPC("RPC_SyncAnimationState", RpcTarget.Others, currentSpeed, currentSpeed > 0.1f, !isGrounded);
                        lastSyncedSpeed = currentSpeed;
                        lastSyncTime = Time.time;

                        if (debugLogs)
                        {
                            Debug.Log($"[AnimSync] Enviando Speed: {currentSpeed:F2}, IsRunning: {currentSpeed > 0.1f}");
                        }
                    }
                }
            }
        }

        private void SetSpeed(float speed)
        {
            currentSpeed = speed;
            animator.SetFloat(Speed, speed);
            animator.SetBool(IsRunning, speed > 0.1f);
        }

        [PunRPC]
        private void RPC_SyncAnimationState(float speed, bool isRunning, bool isJumping)
        {
            // NOVO: Ignora sincronização se o jogador estiver morto
            if (playerHealth != null && playerHealth.isDead)
            {
                return;
            }

            currentSpeed = speed;
            animator.SetFloat(Speed, speed);
            animator.SetBool(IsRunning, isRunning);
            animator.SetBool(IsJumping, isJumping);

            if (debugLogs)
            {
                Debug.Log($"[AnimSync] Recebido Speed: {speed:F2}, IsRunning: {isRunning}, IsJumping: {isJumping}");
            }
        }

        private void UpdateWeaponAnimations()
        {
            if (weaponSystem == null) return;

            int currentWeaponType = weaponSystem.GetCurrentWeaponType() == Combat.WeaponType.Melee ? 0 : 1;
            animator.SetInteger(WeaponType, currentWeaponType);
        }

        public void PlayMeleeAttack()
        {
            if (animator != null)
            {
                animator.SetTrigger(AttackMelee);
                photonView.RPC("RPC_PlayMeleeAttack", RpcTarget.Others);
            }
        }

        public void PlayRangedAttack()
        {
            if (animator != null)
            {
                animator.SetTrigger(AttackRanged);
                photonView.RPC("RPC_PlayRangedAttack", RpcTarget.Others);
            }
        }

        public void PlayDeathAnimation()
        {
            if (animator != null)
            {
                // NOVO: Força todos os parâmetros ao iniciar animação de morte
                animator.SetFloat(Speed, 0f);
                animator.SetBool(IsRunning, false);
                animator.SetBool(IsJumping, false); // CRÍTICO: Desativa IsJumping
                animator.SetBool(IsDead, true);

                photonView.RPC("RPC_PlayDeath", RpcTarget.Others);

                Debug.Log($"[PlayerAnimationController] Animacao de morte ativada para {photonView.Owner.NickName}");
            }
        }

        public void ResetDeathAnimation()
        {
            if (animator != null)
            {
                animator.SetBool(IsDead, false);
                animator.SetBool(IsJumping, false); // NOVO: Garante reset
                photonView.RPC("RPC_ResetDeath", RpcTarget.Others);

                Debug.Log($"[PlayerAnimationController] Animacao de morte resetada para {photonView.Owner.NickName}");
            }
        }

        [PunRPC]
        private void RPC_PlayMeleeAttack()
        {
            if (animator != null)
                animator.SetTrigger(AttackMelee);
        }

        [PunRPC]
        private void RPC_PlayRangedAttack()
        {
            if (animator != null)
                animator.SetTrigger(AttackRanged);
        }

        [PunRPC]
        private void RPC_PlayDeath()
        {
            if (animator != null)
            {
                // NOVO: Força todos os parâmetros remotamente também
                animator.SetFloat(Speed, 0f);
                animator.SetBool(IsRunning, false);
                animator.SetBool(IsJumping, false); // CRÍTICO
                animator.SetBool(IsDead, true);
            }
        }

        [PunRPC]
        private void RPC_ResetDeath()
        {
            if (animator != null)
            {
                animator.SetBool(IsDead, false);
                animator.SetBool(IsJumping, false); // NOVO: Garante reset remoto
            }
        }
    }
}