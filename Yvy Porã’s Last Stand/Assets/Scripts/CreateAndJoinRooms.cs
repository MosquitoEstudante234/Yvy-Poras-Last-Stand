using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using WebSocketSharp;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    string roomName;

    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    public void CreateRoom()
    {
        if (roomName.IsNullOrEmpty() == true)
        {
            roomName = "Room" + Random.Range(0, 100) + Random.Range(0, 100);
        }
        else 
        {
            roomName = createInput.text;
        }
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 2 });
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        roomName = "Room" + Random.Range(0, 100) + Random.Range(0, 100);
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Entrou em " + roomName);
        PhotonNetwork.LoadLevel("Game");
    }
}
