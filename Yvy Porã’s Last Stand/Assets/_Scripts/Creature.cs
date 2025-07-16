using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class Creature : MonoBehaviour
{
    public UnityEvent OnChasing;

    private NavMeshAgent agent;
    private Transform targetPlayer;

    public float checkInterval = 1.5f;
    public string playerTag = "Player";

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(UpdateTarget());
        OnChasing?.Invoke();
    }

    private void Update()
    {
        if (targetPlayer != null)
        {
            agent.SetDestination(targetPlayer.position);
        }
    }

    IEnumerator UpdateTarget()
    {
        while (true)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
            float closestDistance = Mathf.Infinity;
            Transform closest = null;

            foreach (GameObject player in players)
            {
                if (!player.activeInHierarchy) continue;

                var health = player.GetComponent<PlayerHealth>();
                if (health == null || health.isDead) continue;

                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closest = player.transform;
                }
            }

            targetPlayer = closest;
            yield return new WaitForSeconds(checkInterval);
        }
    }
}
