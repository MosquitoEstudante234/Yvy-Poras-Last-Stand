using UnityEngine;

public class InteractActivator : MonoBehaviour
    {
    [Header("Configuração de Interação")]
    public Camera fpsCam;
    public float interactDistance = 3f;
    public string interactionTag = "Interactable";

    [Header("Objetos para alternar")]
    public GameObject objectA;
    public GameObject objectB;

    private bool isToggled = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, interactDistance))
        {
            if (hit.collider.CompareTag(interactionTag))
            {
                ToggleObjects();
            }
        }
    }

    void ToggleObjects()
    {
        isToggled = !isToggled;

        if (objectA != null) objectA.SetActive(!isToggled);
        if (objectB != null) objectB.SetActive(isToggled);

        Debug.Log("Objetos alternados!");
    }
}
