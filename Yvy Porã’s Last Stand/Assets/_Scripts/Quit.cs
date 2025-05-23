using UnityEngine;

public class Quit : MonoBehaviour
{
    
    public void QuitButton()
    {
        Application.Quit();
        Debug.Log("Saindo...");
    }
 
    public void ReplayButton()
    {
        Time.timeScale = 1;
    }
}