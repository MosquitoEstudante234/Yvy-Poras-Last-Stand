using Photon.Pun;
using UnityEngine;

public class Enemy : MonoBehaviourPun
{
    public int maxHealth = 50;
    private int currentHealth;

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
        OnDeath?.Invoke();
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
