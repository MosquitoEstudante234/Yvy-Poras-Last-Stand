using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Enemy : MonoBehaviourPun
{
    public int maxHealth = 50;
    [SerializeField] private int currentHealth;

    public int damage = 10;
    public float initialCooldown = 1f;
    public float damageInterval = 2f;

    private Coroutine damageCoroutine;
    private PlayerHealth currentTarget;

    public delegate void DeathDelegate();
    public event DeathDelegate OnDeath;

    private Renderer enemyRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            enemyRenderer.material = new Material(enemyRenderer.material);
            originalColor = enemyRenderer.material.color;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            enemyRenderer.material.color = originalColor;
        }
    }

    void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    
    public void OnPlayerEnterTrigger(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                currentTarget = player;
                damageCoroutine = StartCoroutine(DealDamageOverTime(player));
            }
        }
    }

    public void OnPlayerExitTrigger(Collider other)
    {
        if (other.CompareTag("Player") && currentTarget != null)
        {
            StopCoroutine(damageCoroutine);
            currentTarget = null;
        }
    }

    IEnumerator DealDamageOverTime(PlayerHealth player)
    {
        yield return new WaitForSeconds(initialCooldown);

        while (player != null && currentTarget == player)
        {
            player.TakeDamage(damage);
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
