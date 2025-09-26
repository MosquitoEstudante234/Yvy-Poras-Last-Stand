using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviourPun
{
    public int damage = 10;
    public float lifeTime = 5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null && !playerHealth.isDead)
            {
                playerHealth.TakeDamage(damage); 
            }

            PhotonNetwork.Destroy(gameObject); // destrói o projétil em todos os clientes
        }
        else
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
