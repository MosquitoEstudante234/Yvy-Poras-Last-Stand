using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToCutscene : MonoBehaviour
{

    public void LoadCutScene()
    {
        SceneManager.LoadScene("Cutscene");
    }

    public void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
