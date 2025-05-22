using UnityEngine;

public class CursorState : MonoBehaviour
{

    public bool LockIt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (LockIt)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

      
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
