using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    string roomName;

    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(createInput.text))
        {
            roomName = " " + Random.Range(0, 100) + Random.Range(0, 100);
            RoomInfoHolder.RoomName = roomName;
        }
        else
        {
            roomName = createInput.text;
        }
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 2 });
    }

    public void JoinRoom()
    {
        roomName = joinInput.text;
        PhotonNetwork.JoinRoom(roomName);
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        roomName = " " + Random.Range(0, 100) + Random.Range(0, 100);
        RoomInfoHolder.RoomName = roomName;
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        RoomInfoHolder.RoomName = roomName;
        Debug.Log("Entrou em " + roomName);
        PhotonNetwork.LoadLevel("Game");
    }
}
