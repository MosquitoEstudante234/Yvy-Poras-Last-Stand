using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    public int damage = 25; // Dano normal
    public float cooldownTime = 3f; // Tempo de cooldown entre ataques
    public float attackRange = 2f; // Alcance do golpe
    public LayerMask enemyLayer; // Camada dos inimigos

    private bool canAttack = true; // Verifica se a lança pode atacar

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack) // Clique esquerdo do mouse
        {
            Attack();
        }
    }

    void Attack()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, attackRange, enemyLayer);

        if (hits.Length > 0)
        {
            bool firstHit = true; // Marca o primeiro inimigo atingido

            foreach (RaycastHit hit in hits)
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    if (firstHit)
                    {
                        enemy.TakeDamage(damage); // Dano total no primeiro inimigo
                        firstHit = false;
                    }
                    else
                    {
                        enemy.TakeDamage(damage / 2); // 50% do dano nos outros
                    }
                }
            }

            Debug.Log("Ataque acertou " + hits.Length + " inimigos.");
        }
        else
        {
            Debug.Log("Ataque errou.");
        }

        StartCoroutine(StartCooldown()); // Cooldown acontece mesmo errando
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
