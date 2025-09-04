using Photon.Pun;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public void Awake()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.LoadLevel("Menu");
    }
}
