using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private Text victoryText;

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
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Encontrar bases
            BaseController[] bases = FindObjectsOfType<BaseController>();
            foreach (BaseController baseCtrl in bases)
            {
                if (baseCtrl.BaseTeam == Team.Indigenous)
                    indigenousBase = baseCtrl;
                else if (baseCtrl.BaseTeam == Team.Portuguese)
                    portugueseBase = baseCtrl;
            }

            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
            {
                Team playerTeam = (Team)((int)teamValue);
                Vector3 spawnPosition = playerTeam == Team.Indigenous
                    ? indigenousBase.PlayerSpawnPoint.position
                    : portugueseBase.PlayerSpawnPoint.position;

                PhotonNetwork.Instantiate(
                    playerPrefab.name,
                    spawnPosition,
                    Quaternion.identity
                );
            }
        }

        public void EndGame(Team winnerTeam)
        {
            victoryUI.SetActive(true);
            victoryText.text = $"{winnerTeam} venceu a partida!";

            // Desabilitar controles
            PlayerController localPlayer = FindObjectOfType<PlayerController>();
            if (localPlayer != null && localPlayer.GetComponent<PhotonView>().IsMine)
            {
                localPlayer.enabled = false;
            }
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