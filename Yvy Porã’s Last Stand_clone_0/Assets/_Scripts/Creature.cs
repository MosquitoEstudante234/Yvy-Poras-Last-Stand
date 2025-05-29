using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class Creature : MonoBehaviour
{
    public UnityEvent OnChasing;

    private NavMeshAgent agent;
    private Transform playerPos;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Encontra o jogador pela tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerPos = player.transform;
            OnChasing.Invoke(); // Invoca o evento apenas se encontrou o player
        }
        else
        {
            Debug.LogWarning("Nenhum objeto com a tag 'Player' foi encontrado.");
        }
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
