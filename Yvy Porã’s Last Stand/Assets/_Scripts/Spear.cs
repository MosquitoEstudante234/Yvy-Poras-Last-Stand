using System.Collections;
using UnityEngine;

public class Spear : MonoBehaviour
{
    public int damage = 25;
    public float cooldownTime = 3f;
    public float attackRange = 2f;
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
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, attackRange, enemyLayer);

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
                        enemy.TakeDamage(damage);
                        firstHit = false;
                    }
                    else
                    {
                        enemy.TakeDamage(damage / 2);
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
        yield return new WaitForSeconds(cooldownTime);
        canAttack = true;
    }
}
