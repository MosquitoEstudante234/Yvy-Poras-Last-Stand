using UnityEngine;
using Photon.Pun;

public class Enemy : MonoBehaviourPun
{
    public int maxHealth = 50;
    [SerializeField] private int currentHealth;

    public delegate void DeathDelegate();
    public event DeathDelegate OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Apenas o host aplica o dano

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (!PhotonNetwork.IsMasterClient) return; // Apenas o host executa a morte

        OnDeath?.Invoke(); // Notifica o WaveSpawner

        PhotonNetwork.Destroy(gameObject); // Destroi sincronizado
    }
}
