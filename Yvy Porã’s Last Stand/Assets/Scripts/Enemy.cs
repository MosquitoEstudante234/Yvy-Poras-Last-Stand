using UnityEngine;

public class Enemy : MonoBehaviour
{
    public delegate void DeathDelegate();
    public event DeathDelegate OnDeath;

    public void Die()
    {
        OnDeath?.Invoke(); // Notifica que o inimigo morreu
        Destroy(gameObject);
    }
}
