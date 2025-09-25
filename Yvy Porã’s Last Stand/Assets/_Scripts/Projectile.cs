using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviourPun
{
    private int damage;

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }

            PhotonNetwork.Destroy(gameObject); // destroi o projétil na rede
        }
        else if (!other.CompareTag("Enemy")) // não colidir com outros inimigos
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
