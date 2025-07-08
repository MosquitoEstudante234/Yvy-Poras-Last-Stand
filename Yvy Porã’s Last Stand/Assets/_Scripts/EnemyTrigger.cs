using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    private Enemy enemy;

    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    void OnTriggerEnter(Collider other)
    {
        enemy?.OnPlayerEnterTrigger(other);
    }

    void OnTriggerExit(Collider other)
    {
        enemy?.OnPlayerExitTrigger(other);
    }
}
