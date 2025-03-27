using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 50;
    [SerializeField]private int currentHealth;

    public delegate void DeathDelegate();
    public event DeathDelegate OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        OnDeath?.Invoke(); // Notifica que o inimigo morreu
        Destroy(gameObject);
       // WaveSpawner.instance.enemiesAlive--;
    }
}
