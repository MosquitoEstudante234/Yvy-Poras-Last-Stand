using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public GameObject npcCanvas; // arraste o Canvas do NPC aqui no inspetor
    private bool playerInside = false;

    void Start()
    {
        if (npcCanvas != null)
            npcCanvas.SetActive(false);
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            ToggleCanvas(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            ToggleCanvas(false);
        }
    }

    void ToggleCanvas(bool state)
    {
        if (npcCanvas != null)
        {
            npcCanvas.SetActive(state);
            Cursor.visible = state;
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
           // if (draft != null)
              //  draft.ShowPendingCards();
        }
    }
}
