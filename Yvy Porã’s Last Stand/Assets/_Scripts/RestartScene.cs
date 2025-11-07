using UnityEngine;
using UnityEngine.Events;

public class RestartScene : MonoBehaviour
{
    [SerializeField] UnityEvent OnRestartScene;
    private void Start()
    {
        OnRestartScene.Invoke();
    }
}
