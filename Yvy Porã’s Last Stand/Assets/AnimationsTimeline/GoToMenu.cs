using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMenu : MonoBehaviour
{
    public bool goToMenuOnEnable = false; 

    private void OnEnable()
    {
        if (goToMenuOnEnable)
        {
            SceneManager.LoadScene("LoadingScene");
        }
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("LoadingScene");
    }
}
