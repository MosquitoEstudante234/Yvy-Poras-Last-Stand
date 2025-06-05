using UnityEngine;
using TMPro;

public class Gun : MonoBehaviour
{
    [Header("Munição")]
    public int maxAmmo = 5;
    private int currentAmmo;

    [Header("Cooldown e Dano")]
    public float cooldownTime = 0.5f;
    private bool canShoot = true;
    public float range = 100f;
    public int damage = 20;

    [Header("Referências")]
    public Camera fpsCam;
    public TextMeshProUGUI ammoText;

    private bool nearReloadZone = false;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && canShoot && currentAmmo > 0)
        {
            Shoot();
        }

        if (nearReloadZone && Input.GetKeyDown(KeyCode.E))
        {
            Reload();
        }
    }

    void Shoot()
    {
        canShoot = false;
        currentAmmo--;
        UpdateAmmoUI();

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Acertou: " + hit.transform.name);

            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        Invoke(nameof(ResetCooldown), cooldownTime);
    }

    void ResetCooldown()
    {
        canShoot = true;
    }

    void Reload()
    {
        currentAmmo = maxAmmo;
        Debug.Log("Estilingue recarregado!");
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"Munição: {currentAmmo}/{maxAmmo}";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ReloadZone"))
        {
            nearReloadZone = true;
            Debug.Log("Pressione E para recarregar o estilingue.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ReloadZone"))
        {
            nearReloadZone = false;
        }
    }
}
