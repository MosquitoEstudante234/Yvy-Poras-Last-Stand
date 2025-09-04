using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject menu;
    bool isOpen;

    void Update()
    {
        if(Input.GetButtonDown("Cancel") && !isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            menu.SetActive(true);
            isOpen = true;
        }

        else if(Input.GetButtonDown("Cancel") && isOpen)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            menu.SetActive(false);
            isOpen = false;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Lobby()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("LoadingScene");
    }

    public void Menu()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("PlayMenu");
    }
}
