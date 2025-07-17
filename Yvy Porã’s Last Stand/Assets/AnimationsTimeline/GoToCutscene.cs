using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToCutscene : MonoBehaviour
{

    public void LoadCutScene()
    {
        SceneManager.LoadScene("Cutscene");
    }
}
