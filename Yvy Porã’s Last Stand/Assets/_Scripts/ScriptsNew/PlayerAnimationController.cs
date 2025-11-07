using MOBAGame.Combat;
using MOBAGame.Core;
using Photon.Pun;
using UnityEngine;

namespace MOBAGame.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviourPun
    {
        private Animator animator;
        private PlayerController playerController;
        private PlayerHealth playerHealth;
        private WeaponSystem weaponSystem;

        // Animation Parameter Names (Hash para performance)
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsJumping = Animator.StringToHash("IsJumping");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        private static readonly int AttackMelee = Animator.StringToHash("AttackMelee");
        private static readonly int AttackRanged = Animator.StringToHash("AttackRanged");
        private static readonly int WeaponType = Animator.StringToHash("WeaponType"); // 0 = Melee, 1 = Ranged
        private static readonly int Speed = Animator.StringToHash("Speed");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            playerController = GetComponent<PlayerController>();
            playerHealth = GetComponent<PlayerHealth>();
            weaponSystem = GetComponent<WeaponSystem>();
        }

        private void Update()
        {
            if (!photonView.IsMine) return;

            UpdateMovementAnimations();
            UpdateWeaponAnimations();
        }

        /// <summary>
        /// Atualiza animações de movimento (correr, idle, pular)
        /// </summary>
        private void UpdateMovementAnimations()
        {
            if (playerController == null || playerHealth == null) return;

            // Verifica se está morto
            if (playerHealth.isDead)
            {
                animator.SetBool(IsDead, true);
                animator.SetBool(IsRunning, false);
                animator.SetFloat(Speed, 0f);
                return;
            }
            else
            {
                animator.SetBool(IsDead, false);
            }

            // Calcula velocidade de movimento
            Vector3 velocity = playerController.GetVelocity();
            float speed = new Vector3(velocity.x, 0, velocity.z).magnitude;

            // Atualiza parâmetros
            animator.SetFloat(Speed, speed);
            animator.SetBool(IsRunning, speed > 0.1f);

            // Pulo (verifica se está no ar)
            bool isGrounded = playerController.IsGrounded();
            animator.SetBool(IsJumping, !isGrounded);
        }

        /// <summary>
        /// Atualiza animações de arma (tipo de arma equipada)
        /// </summary>
        private void UpdateWeaponAnimations()
        {
            if (weaponSystem == null) return;

            // 0 = Melee, 1 = Ranged
            int currentWeaponType = weaponSystem.GetCurrentWeaponType() == Combat.WeaponType.Melee ? 0 : 1;
            animator.SetInteger(WeaponType, currentWeaponType);
        }

        /// <summary>
        /// Trigger de ataque Melee (chamado pelo WeaponSystem)
        /// </summary>
        public void PlayMeleeAttack()
        {
            if (animator != null)
            {
                animator.SetTrigger(AttackMelee);
                photonView.RPC("RPC_PlayMeleeAttack", RpcTarget.Others);
            }
        }

        /// <summary>
        /// Trigger de ataque Ranged (chamado pelo WeaponSystem)
        /// </summary>
        public void PlayRangedAttack()
        {
            if (animator != null)
            {
                animator.SetTrigger(AttackRanged);
                photonView.RPC("RPC_PlayRangedAttack", RpcTarget.Others);
            }
        }

        /// <summary>
        /// Trigger de morte (chamado pelo PlayerHealth)
        /// </summary>
        public void PlayDeathAnimation()
        {
            if (animator != null)
            {
                animator.SetBool(IsDead, true);
                photonView.RPC("RPC_PlayDeath", RpcTarget.Others);
            }
        }

        /// <summary>
        /// Reseta animação de morte no respawn (chamado pelo PlayerHealth)
        /// </summary>
        public void ResetDeathAnimation()
        {
            if (animator != null)
            {
                animator.SetBool(IsDead, false);
                photonView.RPC("RPC_ResetDeath", RpcTarget.Others);
            }
        }

        // ==================== RPCs para Sincronização ====================

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
                animator.SetBool(IsDead, true);
        }

        [PunRPC]
        private void RPC_ResetDeath()
        {
            if (animator != null)
                animator.SetBool(IsDead, false);
        }
    }
}