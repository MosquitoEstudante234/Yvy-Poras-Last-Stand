using UnityEngine;
using Photon.Pun;
public class DisableNotMine : MonoBehaviour
{
    PhotonView phView;
    public GameObject canvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        phView = GetComponentInParent<PhotonView>();
        if (!phView.IsMine)
        {
            gameObject.SetActive(false);
            canvas.SetActive(false);
        }
    }
}
