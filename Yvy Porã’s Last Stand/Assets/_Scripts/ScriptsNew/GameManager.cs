using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using MOBAGame.Lobby;
using MOBAGame.Player;
using MOBAGame.Core;

namespace MOBAGame
{
    public class GameManager : MonoBehaviourPun
    {
        public static GameManager Instance { get; private set; }

        [Header("Player Prefabs")] // ALTERADO
        [SerializeField] private GameObject indigenousPlayerPrefab; // NOVO
        [SerializeField] private GameObject portuguesePlayerPrefab; // NOVO

        [Header("UI")]
        [SerializeField] private GameObject victoryUI;
        [SerializeField] private TextMeshProUGUI victoryText;

        [Header("Bases")]
        private BaseController indigenousBase;
        private BaseController portugueseBase;

        [Header("Spawn Points")]
        public Transform indigenousSpawnPoint;
        public Transform portugueseSpawnPoint;

        public static GameManager instance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

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
                if (baseCtrl.baseTeam == Team.Indigenous)
                    indigenousBase = baseCtrl;
                else if (baseCtrl.baseTeam == Team.Portuguese)
                    portugueseBase = baseCtrl;
            }

            if (indigenousBase == null || portugueseBase == null)
            {
                Debug.LogError("GameManager: Não foram encontradas ambas as bases na cena!");
            }

            SpawnPlayer();
        }

        /// <summary>
        /// Spawna o jogador no ponto de spawn do seu time com o modelo correto
        /// </summary>
        private void SpawnPlayer()
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
            {
                Team playerTeam = (Team)((int)teamValue);

                // Determina qual prefab usar baseado no time
                GameObject prefabToSpawn = null;
                Vector3 spawnPosition = Vector3.zero;
                Quaternion spawnRotation = Quaternion.identity;

                if (playerTeam == Team.Indigenous)
                {
                    prefabToSpawn = indigenousPlayerPrefab; // ALTERADO

                    if (indigenousBase != null && indigenousBase.PlayerSpawnPoint != null)
                    {
                        spawnPosition = indigenousBase.PlayerSpawnPoint.position;
                        spawnRotation = indigenousBase.PlayerSpawnPoint.rotation;
                    }
                    else if (indigenousSpawnPoint != null)
                    {
                        spawnPosition = indigenousSpawnPoint.position;
                        spawnRotation = indigenousSpawnPoint.rotation;
                    }
                }
                else if (playerTeam == Team.Portuguese)
                {
                    prefabToSpawn = portuguesePlayerPrefab; // ALTERADO

                    if (portugueseBase != null && portugueseBase.PlayerSpawnPoint != null)
                    {
                        spawnPosition = portugueseBase.PlayerSpawnPoint.position;
                        spawnRotation = portugueseBase.PlayerSpawnPoint.rotation;
                    }
                    else if (portugueseSpawnPoint != null)
                    {
                        spawnPosition = portugueseSpawnPoint.position;
                        spawnRotation = portugueseSpawnPoint.rotation;
                    }
                }

                // Valida se o prefab foi atribuído
                if (prefabToSpawn != null)
                {
                    PhotonNetwork.Instantiate(
                        prefabToSpawn.name,
                        spawnPosition,
                        spawnRotation
                    );
                }
                else
                {
                    Debug.LogError($"GameManager: Prefab do time {playerTeam} não está configurado!");
                }
            }
            else
            {
                Debug.LogError("GameManager: Jogador não tem time definido nas CustomProperties!");
            }
        }

        public void EndGame(Team winnerTeam)
        {
            if (victoryUI != null)
                victoryUI.SetActive(true);

            if (victoryText != null)
                victoryText.text = $"{winnerTeam} venceu a partida!";

            PlayerController localPlayer = FindObjectOfType<PlayerController>();
            if (localPlayer != null && localPlayer.GetComponent<PhotonView>().IsMine)
            {
                localPlayer.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public BaseController GetBaseForTeam(Team team)
        {
            return team == Team.Indigenous ? indigenousBase : portugueseBase;
        }

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