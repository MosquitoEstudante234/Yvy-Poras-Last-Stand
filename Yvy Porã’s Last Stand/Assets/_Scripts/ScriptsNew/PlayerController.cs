using MOBAGame.Core;
using NUnit.Framework;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviourPun
    {
        [SerializeField]
        public List<GameObject> objectsToDeactivateWhenMine = new List<GameObject>();

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Camera Settings")]
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float verticalLookLimit = 80f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private float verticalRotation = 0f;
        private Team playerTeam = Team.None;

        private void Start()
        {
            controller = GetComponent<CharacterController>();

            if (!photonView.IsMine)
            {
                if (playerCamera != null)
                    playerCamera.gameObject.SetActive(false);

                enabled = false;
                return;
            }
            else
            {
                for (int i = 0; i <= objectsToDeactivateWhenMine.Count; i++)
                {
                    objectsToDeactivateWhenMine[i].SetActive(false);
                }
            }

                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
                {
                    playerTeam = (Team)((int)teamValue);
                }
            

                Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // VALIDAÇÃO: Verifica se o GroundCheck está configurado
            if (groundCheck == null)
            {
                Debug.LogError("PlayerController: GroundCheck NÃO está configurado no Inspector!");
            }
            else
            {
                Debug.Log($"PlayerController: GroundCheck OK - Position: {groundCheck.position}");
            }

            // VALIDAÇÃO: Verifica a layer mask
            Debug.Log($"PlayerController: Ground Mask configurada para layers: {LayerMaskToString(groundMask)}");
            }

        private void Update()
        {
            if (!photonView.IsMine) return;

            HandleGroundCheck();
            HandleMovement();
            HandleCamera();
            HandleJump();
        }

        private void HandleGroundCheck()
        {
            if (groundCheck == null)
            {
                Debug.LogWarning("PlayerController: GroundCheck é NULL!");
                isGrounded = false;
                return;
            }

            // Verifica se está no chão
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            // DEBUG VISUAL: Desenha raio no Scene View
            Debug.DrawRay(groundCheck.position, Vector3.down * groundDistance,
                isGrounded ? Color.green : Color.red);

            // DEBUG: Log a cada segundo (evita spam)
            if (Time.frameCount % 60 == 0) // A cada ~1 segundo (60 FPS)
            {
                Debug.Log($"IsGrounded: {isGrounded}, Velocity.y: {velocity.y:F2}");
            }

            // Reseta velocidade vertical quando tocar o chão
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

            controller.Move(move.normalized * currentSpeed * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleCamera()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

            if (playerCamera != null)
            {
                playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            }
        }

        private void HandleJump()
        {
            // DEBUG: Verifica input de pulo
            bool jumpPressed = Input.GetButtonDown("Jump");

            if (jumpPressed)
            {
                Debug.Log($"[JUMP INPUT] Espaço pressionado! IsGrounded: {isGrounded}");
            }

            if (jumpPressed && isGrounded)
            {
                float jumpVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
                velocity.y = jumpVelocity;

                Debug.Log($"[JUMP SUCCESS] PULOU! Velocity.y definida para: {jumpVelocity:F2}");
            }
            else if (jumpPressed && !isGrounded)
            {
                Debug.LogWarning("[JUMP FAILED] Tentou pular mas NÃO está no chão!");
            }
        }

        public Vector3 GetVelocity()
        {
            return controller.velocity;
        }

        public bool IsGrounded()
        {
            return isGrounded;
        }

        public Team GetTeam()
        {
            return playerTeam;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus && photonView.IsMine)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                // Muda de cor baseado no estado (verde = no chão, vermelho = no ar)
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);

                // Desenha linha mostrando a direção
                Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundDistance);
            }
            else
            {
                // Se não tiver GroundCheck, desenha aviso vermelho
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            }
        }

        // MÉTODO AUXILIAR: Converte LayerMask para string legível
        private string LayerMaskToString(LayerMask mask)
        {
            string result = "";
            for (int i = 0; i < 32; i++)
            {
                if ((mask.value & (1 << i)) != 0)
                {
                    result += LayerMask.LayerToName(i) + ", ";
                }
            }
            return string.IsNullOrEmpty(result) ? "NENHUMA" : result.TrimEnd(',', ' ');
        }
    }
}