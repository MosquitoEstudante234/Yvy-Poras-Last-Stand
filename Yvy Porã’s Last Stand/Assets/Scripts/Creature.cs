using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class Creature : MonoBehaviour
{
    public UnityEvent OnChasing;

    private NavMeshAgent agent;
    public Transform playerPos;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        OnChasing.Invoke();
    }

    private void Update()
    {
        ChasePlayer();
    }

    private void ChasePlayer()
    {
        if (playerPos != null)
        {
            agent.SetDestination(playerPos.position);
        }
    }
}