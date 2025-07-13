using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DisplayRoomName : MonoBehaviourPunCallbacks
{
    public TMP_Text roomNameText;

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            roomNameText.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
        }
    }
}
