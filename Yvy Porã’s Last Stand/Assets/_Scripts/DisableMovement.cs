using UnityEngine;
using Photon.Pun;
public class DisableMovement : MonoBehaviour
{
    PhotonView phView;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        phView = GetComponentInParent<PhotonView>();
        if (!phView.IsMine)
        {
            gameObject.SetActive(false);
        }
    }
}
