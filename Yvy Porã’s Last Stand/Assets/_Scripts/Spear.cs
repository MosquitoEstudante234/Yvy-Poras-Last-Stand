using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    public int damage = 25; 
    public float cooldownTime = 0.4f; 
    public float attackRange = 4f; 
    public LayerMask enemyLayer; 

    private bool canAttack = true; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack) 
        {
            Attack();
        }
    }

    void Attack()
    {
       
        int mask = ~LayerMask.GetMask("EnemyIgnore");

        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, attackRange, mask);

        if (hits.Length > 0)
        {
            bool firstHit = true; 

            foreach (RaycastHit hit in hits)
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    if (firstHit)
                    {
                        enemy.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, damage);
                        firstHit = false;
                    }
                    else
                    {
                        enemy.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, damage / 2); 
                    }
                }
            }

            Debug.Log("Ataque acertou " + hits.Length + " inimigos.");
        }
        else
        {
            Debug.Log("Ataque errou.");
        }

        StartCoroutine(StartCooldown()); 
    }

    IEnumerator StartCooldown()
    {
        canAttack = false;
        Debug.Log("Cooldown iniciado.");
        yield return new WaitForSeconds(cooldownTime);
        canAttack = true;
        Debug.Log("Cooldown finalizado.");
    }
}
