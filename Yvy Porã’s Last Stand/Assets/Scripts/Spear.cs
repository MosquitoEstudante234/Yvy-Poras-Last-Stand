using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    public int damage = 25; // Dano causado ao inimigo
    public float cooldownTime = 3f; // Tempo de cooldown entre ataques
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
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 1f); // Detecta inimigos próximos

        foreach (Collider enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log("Deu Dano");
                enemy.TakeDamage(damage);
                StartCoroutine(StartCooldown());
                break; // Garante que só atacamos um inimigo por clique
            }
        }
    }

    IEnumerator StartCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(cooldownTime);
        canAttack = true;
        Debug.Log("Cooldown");
    }
}
