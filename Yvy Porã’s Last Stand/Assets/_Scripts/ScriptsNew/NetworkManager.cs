using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace MOBAGame.Network
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance { get; private set; }

        [Header("Connection Settings")]
        [SerializeField] private string gameVersion = "1.0";
        [SerializeField] private byte maxPlayersPerRoom = 2;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            ConnectToPhoton();
        }

        public void ConnectToPhoton()
        {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
                Debug.Log("[NetworkManager] Conectando ao Photon...");
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("[NetworkManager] Conectado ao Master Server");
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("[NetworkManager] Entrou no Lobby");
            // Carregar cena de seleção de time
            PhotonNetwork.LoadLevel("LobbyScene");
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarning($"[NetworkManager] Desconectado: {cause}");
        }
    }
}