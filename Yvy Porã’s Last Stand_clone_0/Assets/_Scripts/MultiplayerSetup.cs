using UnityEngine;
using Photon.Pun;

public class MultiplayerSetup : MonoBehaviourPunCallbacks
{
    public GameObject cameraRoot;
    public GameObject spear;

    void Start()
    {
        if (photonView.IsMine)
        {
            // Habilita c�mera e controles apenas para o jogador local
            cameraRoot.SetActive(true);
            spear.SetActive(true); // Ativa a arma do jogador local
        }
        else
        {
            // Desativa c�mera e controles para jogadores remotos
            cameraRoot.SetActive(false);
            spear.SetActive(false);
        }
    }
}
