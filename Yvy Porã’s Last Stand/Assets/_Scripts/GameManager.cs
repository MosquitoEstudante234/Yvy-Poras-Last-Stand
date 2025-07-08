using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    private int playersDead = 0;

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

    public void PlayerDied()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        playersDead++;

        Debug.Log("Players mortos: " + playersDead + " / " + PhotonNetwork.CurrentRoom.PlayerCount);

        if (playersDead >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Debug.Log("Todos os jogadores morreram. Voltando para o menu...");
            photonView.RPC("ReturnToMenu", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
