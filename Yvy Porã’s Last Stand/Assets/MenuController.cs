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

        SceneManager.LoadScene("Menu");
    }

    public void Menu()
    {
        PhotonNetwork.Destroy(gameObject);
        SceneManager.LoadScene("Menu");
    }
}
