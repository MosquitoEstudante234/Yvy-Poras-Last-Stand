using UnityEngine;
using UnityEngine.UI;
using TMPro; // ADICIONADO: TextMeshPro
using Photon.Pun;
using MOBAGame.Lobby;
using MOBAGame.Player;
using MOBAGame.Core;

namespace MOBAGame
{
    public class GameManager : MonoBehaviourPun
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject victoryUI;
        [SerializeField] private TextMeshProUGUI victoryText; // ALTERADO

        [Header("Bases")]
        private BaseController indigenousBase;
        private BaseController portugueseBase;

        [Header("Spawn Points")]
        public Transform indigenousSpawnPoint;
        public Transform portugueseSpawnPoint;

        // NOTA: Mantive 'instance' para compatibilidade, mas Instance já é o Singleton
        public static GameManager instance;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Compatibilidade com código antigo que usa 'instance'
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Encontrar bases na cena
            BaseController[] bases = FindObjectsOfType<BaseController>();
            foreach (BaseController baseCtrl in bases)
            {
                if (baseCtrl.baseTeam == Team.Indigenous) // CORRIGIDO: baseTeam público
                    indigenousBase = baseCtrl;
                else if (baseCtrl.baseTeam == Team.Portuguese)
                    portugueseBase = baseCtrl;
            }

            // Valida se as bases foram encontradas
            if (indigenousBase == null || portugueseBase == null)
            {
                Debug.LogError("GameManager: Não foram encontradas ambas as bases na cena!");
            }

            // Spawna o jogador local
            SpawnPlayer();
        }

        /// <summary>
        /// Spawna o jogador no ponto de spawn do seu time
        /// </summary>
        private void SpawnPlayer()
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
            {
                Team playerTeam = (Team)((int)teamValue);

                // Determina posição de spawn baseado no time
                Vector3 spawnPosition = Vector3.zero;
                Quaternion spawnRotation = Quaternion.identity;

                if (playerTeam == Team.Indigenous && indigenousBase != null)
                {
                    spawnPosition = indigenousBase.PlayerSpawnPoint != null
                        ? indigenousBase.PlayerSpawnPoint.position
                        : indigenousSpawnPoint.position;
                    spawnRotation = indigenousBase.PlayerSpawnPoint != null
                        ? indigenousBase.PlayerSpawnPoint.rotation
                        : indigenousSpawnPoint.rotation;
                }
                else if (playerTeam == Team.Portuguese && portugueseBase != null)
                {
                    spawnPosition = portugueseBase.PlayerSpawnPoint != null
                        ? portugueseBase.PlayerSpawnPoint.position
                        : portugueseSpawnPoint.position;
                    spawnRotation = portugueseBase.PlayerSpawnPoint != null
                        ? portugueseBase.PlayerSpawnPoint.rotation
                        : portugueseSpawnPoint.rotation;
                }

                // Instantiate via Photon (precisa estar em Resources/)
                if (playerPrefab != null)
                {
                    PhotonNetwork.Instantiate(
                        playerPrefab.name,
                        spawnPosition,
                        spawnRotation
                    );
                }
                else
                {
                    Debug.LogError("GameManager: playerPrefab não está configurado!");
                }
            }
            else
            {
                Debug.LogError("GameManager: Jogador não tem time definido nas CustomProperties!");
            }
        }

        /// <summary>
        /// Finaliza o jogo e mostra tela de vitória
        /// </summary>
        public void EndGame(Team winnerTeam)
        {
            if (victoryUI != null)
                victoryUI.SetActive(true);

            if (victoryText != null)
                victoryText.text = $"{winnerTeam} venceu a partida!";

            // Desabilitar controles do jogador local
            PlayerController localPlayer = FindObjectOfType<PlayerController>();
            if (localPlayer != null && localPlayer.GetComponent<PhotonView>().IsMine)
            {
                localPlayer.enabled = false;
            }

            // Opcional: Bloquear inputs globalmente
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Retorna a base do time especificado
        /// </summary>
        public BaseController GetBaseForTeam(Team team)
        {
            return team == Team.Indigenous ? indigenousBase : portugueseBase;
        }

        /// <summary>
        /// Retorna o ponto de spawn do time especificado
        /// </summary>
        public Transform GetSpawnPointForTeam(Team team)
        {
            if (team == Team.Indigenous)
                return indigenousSpawnPoint;
            else if (team == Team.Portuguese)
                return portugueseSpawnPoint;

            return null;
        }
    }
}