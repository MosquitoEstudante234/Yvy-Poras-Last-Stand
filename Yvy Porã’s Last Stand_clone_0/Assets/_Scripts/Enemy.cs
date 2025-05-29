using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 50;
    [SerializeField] private int currentHealth;

    public delegate void DeathDelegate();
    public event DeathDelegate OnDeath;

    private Renderer enemyRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        // Guarda a referência do Renderer e a cor original
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            // Instancia o material para evitar alterar outros inimigos
            enemyRenderer.material = new Material(enemyRenderer.material);
            originalColor = enemyRenderer.material.color;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Pisca vermelho ao tomar dano
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
            enemyRenderer.material.color = Color.red; // Fica vermelho
            yield return new WaitForSeconds(0.2f);    // Espera um tempo curto
            enemyRenderer.material.color = originalColor; // Volta à cor original
        }
    }

    void Die()
    {
        OnDeath?.Invoke(); // Notifica que o inimigo morreu
        Destroy(gameObject);
        // WaveSpawner.instance.enemiesAlive--;
    }
}
