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

    [Header("Recarga por olhar")]
    public float reloadLookDistance = 3f; // Distância máxima para ativar recarga ao olhar
    public string reloadTag = "ReloadZone";

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    void Update()
    {
        // Atirar
        if (Input.GetButtonDown("Fire1") && canShoot && currentAmmo > 0)
        {
            Shoot();
        }

        // Verifica se está olhando para a zona de recarga
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryReload();
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

    void TryReload()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, reloadLookDistance))
        {
            if (hit.collider.CompareTag(reloadTag))
            {
                Reload();
                Debug.Log("Estilingue recarregado ao olhar para o objeto!");
            }
        }
    }

    void Reload()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"Munição: {currentAmmo}/{maxAmmo}";
        }
    }
}
