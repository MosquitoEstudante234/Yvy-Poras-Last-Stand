using UnityEngine;

public class Gun : MonoBehaviour
{
    public float cooldownTime = 0.5f; // Tempo entre tiros
    private bool canShoot = true;

    public float range = 100f;
    public int damage = 20;

    public Camera fpsCam;

    void Update()
    {
        // Dispara apenas se clicou (não segurou) e se pode atirar
        if (Input.GetButtonDown("Fire1") && canShoot)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        canShoot = false;

        // Raycast para detectar acerto
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Acertou: " + hit.transform.name);
            // Ex: aplicar dano → hit.transform.GetComponent<Enemy>()?.TakeDamage(damage);
        }

        // Espera cooldown para permitir novo tiro
        Invoke(nameof(ResetCooldown), cooldownTime);
    }

    void ResetCooldown()
    {
        canShoot = true;
    }
}
