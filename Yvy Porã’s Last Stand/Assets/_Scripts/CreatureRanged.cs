using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CreatureRanged : MonoBehaviour
{
    [Header("Referências")]
    private NavMeshAgent agent;
    private Transform targetPlayer;

    [Header("Configurações de Movimento")]
    public float stopDistance = 5f;   // Distância para parar e atirar
    public float checkInterval = 1.5f;
    public string playerTag = "Player";

    [Header("Ataque à Distância")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float projectileSpeed = 10f;

    private float nextFireTime = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(UpdateTarget());
    }

    private void Update()
    {
        if (targetPlayer == null) return;

        float distance = Vector3.Distance(transform.position, targetPlayer.position);

        if (distance > stopDistance)
        {
            // Persegue o player
            agent.isStopped = false;
            agent.SetDestination(targetPlayer.position);
        }
        else
        {
            // Para para atirar
            agent.isStopped = true;

            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate;
            }
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

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null || targetPlayer == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (targetPlayer.position - firePoint.position).normalized;
            rb.linearVelocity = dir * projectileSpeed;
        }
    }
}
