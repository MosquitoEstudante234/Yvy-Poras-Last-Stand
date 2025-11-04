using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class Projectile : MonoBehaviourPun
{
    public int damage = 5;
    public float lifeTime = 5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            print("Foi");
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null && !playerHealth.isDead)
            {
                playerHealth.TakeDamage(damage);
                gameObject.SetActive(false);
            }

            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}