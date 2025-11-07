using UnityEngine;
using UnityEngine.Events;

public class RestartScene : MonoBehaviour
{
    [SerializeField] UnityEvent OnRestartScene;
    private void Awake()
    {
        OnRestartScene.Invoke();
    }
}
