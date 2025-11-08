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

        // Animation Parameter Names (Hash para performance)
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsJumping = Animator.StringToHash("IsJumping");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        private static readonly int AttackMelee = Animator.StringToHash("AttackMelee");
        private static readonly int AttackRanged = Animator.StringToHash("AttackRanged");
        private static readonly int WeaponType = Animator.StringToHash("WeaponType"); // 0 = Melee, 1 = Ranged
        private static readonly int Speed = Animator.StringToHash("Speed");

        [SerializeField] private float speedSmoothTime = 0.08f;
        private float currentSpeed;
        [Tooltip("Ativa logs detalhados para debug")] 
        public bool debugLogs = true;

        private void Awake()
        {
            if (animator == null)
            {
                // tenta encontrar no filho (muito comum o Animator estar em um child)
                animator = GetComponentInChildren<Animator>();
            }

            if (animator == null)
            {
                Debug.LogError("PlayerAnimationController: Animator não encontrado no Player ou filhos!");
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

            // 1) Pega a velocidade mundial do CharacterController
            Vector3 worldVelocity = Vector3.zero;
            if (characterController != null)
                worldVelocity = characterController.velocity;

            // 2) Converte para velocidade local (relativa à rotação do personagem)
            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

            // MÉTODOS DE CÁLCULO (vai usar o primeiro que fizer sentido)
            // Método A: magnitude horizontal em espaço local (x,z)
            float speedA = new Vector2(localVelocity.x, localVelocity.z).magnitude;

            // Método B: componente forward (z) absoluto — útil se movimentação é só em forward/back
            float speedB = Mathf.Abs(localVelocity.z);

            // Método C: magnitude horizontal em espaço mundial (ignora y)
            Vector3 worldHorizontal = new Vector3(worldVelocity.x, 0f, worldVelocity.z);
            float speedC = worldHorizontal.magnitude;

            // Seleciona o maior valor (fallback robusto)
            float chosenSpeed = Mathf.Max(speedA, speedB, speedC);

            // Se todos zerarem, tenta uma detecção rápida de input (apenas como fallback, caso tu tenha input exposto)
            // OBS: aqui eu não faço reflexão para não arriscar; caso tu tenha um campo público em PlayerController (eg. moveInput),
            // tu pode manualmente atribuir a velocidade baseada no input.
            
            // Suaviza a transição do parâmetro Speed
            currentSpeed = Mathf.Lerp(currentSpeed, chosenSpeed, 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.0001f, speedSmoothTime)));

            // Atualiza Animator
            animator.SetFloat(Speed, currentSpeed);
            animator.SetBool(IsRunning, currentSpeed > 0.1f);

            // Pulo
            bool isGrounded = playerController.IsGrounded();
            animator.SetBool(IsJumping, !isGrounded);

            // DEBUG: imprime valores para diagnosticar
            if (debugLogs)
            {
               // Debug.Log($"[Anim] localVel: {localVelocity}, worldVel: {worldVelocity}, speedA:{speedA:F3}, speedB:{speedB:F3}, speedC:{speedC:F3}, chosen:{chosenSpeed:F3}, current:{currentSpeed:F3}");
            }
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

        // (restante dos métodos RPCs e triggers inalterados)
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
                animator.SetBool(IsDead, true);
                photonView.RPC("RPC_PlayDeath", RpcTarget.Others);
            }
        }

        public void ResetDeathAnimation()
        {
            if (animator != null)
            {
                animator.SetBool(IsDead, false);
                photonView.RPC("RPC_ResetDeath", RpcTarget.Others);
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
