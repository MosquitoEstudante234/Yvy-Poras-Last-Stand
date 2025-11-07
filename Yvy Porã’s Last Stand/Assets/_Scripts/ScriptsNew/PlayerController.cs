using UnityEngine;
using Photon.Pun;
using MOBAGame.Lobby;
using MOBAGame.Core;

namespace MOBAGame.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PhotonView))]
    public class PlayerController : MonoBehaviourPun, IPunObservable
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;

        [Header("Components")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private Team playerTeam;

        // Network sync
        private Vector3 networkPosition;
        private Quaternion networkRotation;

        public Team PlayerTeam => playerTeam;
        public bool IsDead { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            // Obter time do jogador
            if (photonView.IsMine)
            {
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
                {
                    playerTeam = (Team)((int)teamValue);
                }

                // Configurar câmera local
                if (cameraTransform != null)
                {
                    cameraTransform.gameObject.SetActive(true);
                }
            }
            else
            {
                // Desabilitar câmera de outros jogadores
                if (cameraTransform != null)
                {
                    cameraTransform.gameObject.SetActive(false);
                }

                networkPosition = transform.position;
                networkRotation = transform.rotation;
            }
        }

        private void Update()
        {
            if (photonView.IsMine && !IsDead)
            {
                HandleMovement();
                HandleJump();
            }
            else
            {
                // Interpolação suave para outros jogadores
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
            }
        }

        private void HandleMovement()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 move = transform.right * horizontal + transform.forward * vertical;

            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            controller.Move(move * currentSpeed * Time.deltaTime);

            // Rotação da câmera (mouse)
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            transform.Rotate(Vector3.up * mouseX);

            // Aplicar gravidade
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }
        }

        public void Die()
        {
            if (IsDead) return;

            IsDead = true;
            photonView.RPC("RPC_Die", RpcTarget.All);
        }

        [PunRPC]
        private void RPC_Die()
        {
            // Desabilitar controles e collider
            controller.enabled = false;
            GetComponent<Collider>().enabled = false;

            // Trigger animação de morte
            GetComponent<Animator>()?.SetTrigger("Die");
        }

        public void Respawn(Vector3 spawnPosition)
        {
            IsDead = false;
            transform.position = spawnPosition;
            controller.enabled = true;
            GetComponent<Collider>().enabled = true;

            photonView.RPC("RPC_Respawn", RpcTarget.All);
        }

        [PunRPC]
        private void RPC_Respawn()
        {
            GetComponent<Animator>()?.SetTrigger("Respawn");
        }

        // Sincronização Photon
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Enviar dados
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(velocity);
            }
            else
            {
                // Receber dados
                networkPosition = (Vector3)stream.ReceiveNext();
                networkRotation = (Quaternion)stream.ReceiveNext();
                velocity = (Vector3)stream.ReceiveNext();
            }
        }
    }
}