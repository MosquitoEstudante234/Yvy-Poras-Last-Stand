using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    string roomName;

    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    // Controle se o player já está no lobby
    private bool inLobby = false;

    // Botão para criar sala
    public void CreateRoom()
    {
        if (!inLobby)
        {
            Debug.LogError("Ainda não entrou no Lobby!");
            return;
        }

        if (string.IsNullOrEmpty(createInput.text))
        {
            roomName = "Room" + Random.Range(0, 100) + Random.Range(0, 100);
            RoomInfoHolder.RoomName = roomName;
        }
        else
        {
            roomName = createInput.text;
        }

        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 2 });
    }

    // Botão para entrar em sala específica
    public void JoinRoom()
    {
        if (!inLobby)
        {
            Debug.LogError("Ainda não entrou no Lobby!");
            return;
        }

        roomName = joinInput.text;
        PhotonNetwork.JoinRoom(roomName);
    }

    // Botão para entrar em sala aleatória
    public void JoinRandomRoom()
    {
        if (!inLobby)
        {
            Debug.LogError("Ainda não entrou no Lobby!");
            return;
        }

        PhotonNetwork.JoinRandomRoom();
    }

    // Caso falhe ao entrar em sala aleatória, cria uma nova
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        roomName = "Room" + Random.Range(0, 100) + Random.Range(0, 100);
        RoomInfoHolder.RoomName = roomName;
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 2 });
    }

    // Callback quando entra na sala
    public override void OnJoinedRoom()
    {
        RoomInfoHolder.RoomName = roomName;
        Debug.Log("Entrou em " + roomName);
        PhotonNetwork.LoadLevel("Game");
    }

    // Callback quando sai da sala
    public override void OnLeftRoom()
    {
        Debug.Log("Saiu da sala, entrando no lobby...");
        PhotonNetwork.JoinLobby(); // volta para o lobby
    }

    // Callback quando entra no lobby
    public override void OnJoinedLobby()
    {
        Debug.Log("Entrou no Lobby! Agora pode criar ou entrar em salas.");
        inLobby = true;
    }

    // Callback quando desconectado
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Desconectado do Photon: " + cause);
        inLobby = false;
    }
}
