using TMPro;
using UnityEngine;

public class DisplayRoomName : MonoBehaviour
{
    public TMP_Text roomNameText;

    void Start()
    {
        roomNameText.text = "Est�s no quarto: " + RoomInfoHolder.RoomName;
    }
}
