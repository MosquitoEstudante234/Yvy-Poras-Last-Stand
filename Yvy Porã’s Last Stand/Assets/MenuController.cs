using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject menu;
    bool isOpen;

    private static bool hasNotifiedOthers = false; // Previne notificações duplicadas

    private void Start()
    {
        // Reseta flag ao entrar na cena
        hasNotifiedOthers = false;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel") && !isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            menu.SetActive(true);
            isOpen = true;
        }
        else if (Input.GetButtonDown("Cancel") && isOpen)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            menu.SetActive(false);
            isOpen = false;
        }
    }

    public void Quit()
    {
        // Notifica outros jogadores antes de sair
        NotifyOthersAndDisconnect("PlayMenu");

        // Pequeno delay antes de fechar a aplicação para garantir que o RPC seja enviado
        Invoke(nameof(QuitApplication), 0.5f);
    }

    private void QuitApplication()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void Lobby()
    {
        NotifyOthersAndDisconnect("LoadingScene");
    }

    public void Menu()
    {
        NotifyOthersAndDisconnect("PlayMenu");
    }

    /// <summary>
    /// Notifica outros jogadores e desconecta
    /// </summary>
    private void NotifyOthersAndDisconnect(string targetScene)
    {
        if (PhotonNetwork.InRoom && !hasNotifiedOthers)
        {
            hasNotifiedOthers = true;

            Debug.Log($"[MenuController] Notificando outros jogadores que estou saindo para {targetScene}");

            // Envia RPC para todos os outros jogadores
            PhotonView photonView = GetPhotonView();
            if (photonView != null)
            {
                photonView.RPC("RPC_PlayerQuit", RpcTarget.Others, targetScene);
            }

            // Sai da sala e desconecta
            PhotonNetwork.LeaveRoom(false); // false = não se torna inativo
        }

        // Desconecta e carrega a cena
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(targetScene);
    }

    /// <summary>
    /// RPC chamado quando um jogador sai, forçando o outro a desconectar também
    /// </summary>
    [PunRPC]
    private void RPC_PlayerQuit(string targetScene)
    {
        if (hasNotifiedOthers) return; // Previne loop infinito

        hasNotifiedOthers = true;

        Debug.Log($"[MenuController] Outro jogador saiu! Desconectando e indo para {targetScene}");

        // Fecha o menu se estiver aberto
        if (menu != null)
        {
            menu.SetActive(false);
        }

        // Desconecta e retorna à cena especificada
        PhotonNetwork.LeaveRoom(false);
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(targetScene);
    }

    /// <summary>
    /// Callback do Photon quando um jogador deixa a sala
    /// (Funciona quando o outro player fecha o jogo abruptamente - Alt+F4, crash, etc)
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (hasNotifiedOthers) return; // Já está processando saída

        Debug.Log($"[MenuController] Jogador {otherPlayer.NickName} saiu da sala abruptamente");

        hasNotifiedOthers = true;

        // Desconecta o jogador local e retorna ao menu
        PhotonNetwork.LeaveRoom(false);
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("PlayMenu");
    }

    /// <summary>
    /// Callback do Photon quando o MasterClient muda
    /// (Caso o host saia e o Photon tente transferir a role)
    /// </summary>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (hasNotifiedOthers) return; // Já está processando saída

        Debug.Log($"[MenuController] MasterClient mudou para {newMasterClient.NickName} durante a partida");

        hasNotifiedOthers = true;

        // Encerra a sessão para todos
        PhotonNetwork.LeaveRoom(false);
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("PlayMenu");
    }

    /// <summary>
    /// Callback do Photon quando desconecta
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"[MenuController] Desconectado do Photon. Causa: {cause}");
    }

    /// <summary>
    /// Callback do Photon quando deixa a sala com sucesso
    /// </summary>
    public override void OnLeftRoom()
    {
        Debug.Log("[MenuController] Saiu da sala com sucesso");
    }

    /// <summary>
    /// Busca ou cria um PhotonView para este objeto
    /// </summary>
    private PhotonView GetPhotonView()
    {
        PhotonView pv = GetComponent<PhotonView>();

        if (pv == null)
        {
            Debug.LogError("[MenuController] PhotonView não encontrado! Adicione um PhotonView ao GameObject do MenuController");
        }

        return pv;
    }

    /// <summary>
    /// Fallback: Se o jogador fechar o aplicativo, envia notificação antes
    /// </summary>
    private void OnApplicationQuit()
    {
        if (PhotonNetwork.InRoom && !hasNotifiedOthers)
        {
            hasNotifiedOthers = true;

            PhotonView photonView = GetPhotonView();
            if (photonView != null)
            {
                photonView.RPC("RPC_PlayerQuit", RpcTarget.Others, "PlayMenu");
            }

            PhotonNetwork.LeaveRoom(false);
        }
    }
}