using UnityEngine;
using UnityEngine.UI;
using TMPro; // ADICIONADO: TextMeshPro
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using MOBAGame.Core;

namespace MOBAGame.Lobby
{
    public class TeamSelectionManager : MonoBehaviourPunCallbacks
    {
        [Header("UI References")]
        [SerializeField] private Button indigenousButton;
        [SerializeField] private Button portugueseButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button startGameButton;

        [Header("Team Display")]
        [SerializeField] private TextMeshProUGUI indigenousPlayerText; // ALTERADO
        [SerializeField] private TextMeshProUGUI portuguesePlayerText; // ALTERADO
        [SerializeField] private TextMeshProUGUI statusText; // ALTERADO

        [Header("Settings")]
        [SerializeField] private string gameSceneName = "GameScene";

        private const string TEAM_KEY = "Team";
        private const string READY_KEY = "Ready";

        private void Start()
        {
            SetupButtons();
            UpdateTeamDisplay();

            // Apenas o Master Client pode iniciar o jogo
            if (startGameButton != null)
                startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }

        private void SetupButtons()
        {
            indigenousButton?.onClick.AddListener(() => SelectTeam(Team.Indigenous));
            portugueseButton?.onClick.AddListener(() => SelectTeam(Team.Portuguese));
            readyButton?.onClick.AddListener(ToggleReady);
            startGameButton?.onClick.AddListener(StartGame);
        }

        private void SelectTeam(Team team)
        {
            // Verifica se o time já está ocupado
            if (IsTeamOccupied(team))
            {
                if (statusText != null)
                    statusText.text = "Time já ocupado!";
                return;
            }

            // Define propriedades customizadas do jogador
            ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
            {
                { TEAM_KEY, (int)team },
                { READY_KEY, false }
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

            if (statusText != null)
                statusText.text = $"Time selecionado: {team}";
        }

        private bool IsTeamOccupied(Team team)
        {
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if (player.CustomProperties.TryGetValue(TEAM_KEY, out object teamValue))
                {
                    if ((int)teamValue == (int)team && player != PhotonNetwork.LocalPlayer)
                        return true;
                }
            }
            return false;
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            UpdateTeamDisplay();
            UpdateStartButton();
        }

        public void UpdateTeamDisplay()
        {
            if (indigenousPlayerText != null)
                indigenousPlayerText.text = "Indígenas: Aguardando...";

            if (portuguesePlayerText != null)
                portuguesePlayerText.text = "Portugueses: Aguardando...";

            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if (player.CustomProperties.TryGetValue(TEAM_KEY, out object teamValue))
                {
                    bool isReady = player.CustomProperties.TryGetValue(READY_KEY, out object readyValue) && (bool)readyValue;
                    string readyStatus = isReady ? " [PRONTO]" : "";

                    switch ((int)teamValue)
                    {
                        case (int)Team.Indigenous:
                            if (indigenousPlayerText != null)
                                indigenousPlayerText.text = $"Indígenas: {player.NickName}{readyStatus}";
                            break;
                        case (int)Team.Portuguese:
                            if (portuguesePlayerText != null)
                                portuguesePlayerText.text = $"Portugueses: {player.NickName}{readyStatus}";
                            break;
                    }
                }
            }
        }

        private void ToggleReady()
        {
            // Verifica se jogador selecionou um time
            if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(TEAM_KEY, out object teamValue) ||
                (int)teamValue == (int)Team.None)
            {
                if (statusText != null)
                    statusText.text = "Selecione um time primeiro!";
                return;
            }

            bool currentReady = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(READY_KEY, out object readyValue) && (bool)readyValue;

            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { READY_KEY, !currentReady }
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        private void UpdateStartButton()
        {
            if (!PhotonNetwork.IsMasterClient || startGameButton == null) return;

            // Verifica se ambos os times têm jogadores e estão prontos
            bool indigenousReady = false;
            bool portugueseReady = false;

            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if (player.CustomProperties.TryGetValue(TEAM_KEY, out object teamValue) &&
                    player.CustomProperties.TryGetValue(READY_KEY, out object readyValue) &&
                    (bool)readyValue)
                {
                    if ((int)teamValue == (int)Team.Indigenous)
                        indigenousReady = true;
                    else if ((int)teamValue == (int)Team.Portuguese)
                        portugueseReady = true;
                }
            }

            startGameButton.interactable = indigenousReady && portugueseReady;
        }

        private void StartGame()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            PhotonNetwork.LoadLevel(gameSceneName);
        }

        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            if (startGameButton != null)
                startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }
    }
}