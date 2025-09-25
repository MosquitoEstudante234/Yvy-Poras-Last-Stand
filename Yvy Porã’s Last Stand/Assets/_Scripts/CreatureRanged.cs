using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Enemy))] // garante que o inimigo tem vida/dano
public class CreatureRanged : MonoBehaviourPun
{
    public UnityEvent OnChasing;

    private NavMeshAgent agent;
    private Transform targetPlayer;
    private Enemy enemy; // script de vida/dano

    public float checkInterval = 1.5f;
    public string playerTag = "Player";

    [Header("Ataque à distância")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float attackRange = 10f;
    public float fireRate = 2f;
    private float fireCooldown = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();

        StartCoroutine(UpdateTarget());
        OnChasing?.Invoke();
    }

    private void Update()
    {
        if (targetPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, targetPlayer.position);

            if (dist > attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(targetPlayer.position);
            }
            else
            {
                agent.isStopped = true;

                Vector3 dir = (targetPlayer.position - transform.position).normalized;
                dir.y = 0;
                transform.rotation = Quaternion.LookRotation(dir);

                if (fireCooldown <= 0f)
                {
                    photonView.RPC("Shoot", RpcTarget.All, targetPlayer.position);
                    fireCooldown = fireRate;
                }
            }
        }

        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
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

    [PunRPC]
    void Shoot(Vector3 targetPos)
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

            // calcula direção pro player
            Vector3 dir = (targetPos - shootPoint.position).normalized;

            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = dir * 15f; // Unity 6
#else
                rb.velocity = dir * 15f;       // Unity 2022/2023
#endif
            }

            Projectile projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetDamage(enemy.damage); // pega o valor do Enemy.cs
            }
        }
    }
}
