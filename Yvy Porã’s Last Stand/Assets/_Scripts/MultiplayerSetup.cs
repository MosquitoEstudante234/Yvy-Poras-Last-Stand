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
            
            cameraRoot.SetActive(true);
            spear.SetActive(true); 
        }
        else
        {
            
            cameraRoot.SetActive(false);
            spear.SetActive(false);
        }
    }
}
