using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    private int playersDead = 0;
    private int totalPlayers = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            Debug.Log("Total de jogadores na partida: " + totalPlayers);
        }
    }

    public void PlayerDied()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        playersDead++;
        Debug.Log("Players mortos: " + playersDead + " / " + totalPlayers);

        if (playersDead >= totalPlayers)
        {
            Debug.Log("Todos os jogadores morreram. Voltando para o menu...");
            photonView.RPC("ReturnToMenu", RpcTarget.All); // sem Buffered
        }
    }

    [PunRPC]
    void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
