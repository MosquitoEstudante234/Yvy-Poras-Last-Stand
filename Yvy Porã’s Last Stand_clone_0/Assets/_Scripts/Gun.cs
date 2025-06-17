using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    [Header("Munição")]
    public int maxAmmo = 5;
    private int currentAmmo;

    [Header("Cooldown e Dano")]
    public float cooldownTime = 0.5f;
    private bool canShoot = true;
    public float range = 100f;
    public int maxDamage = 20;

    [Header("Carregamento")]
    public float chargeDuration = 1.5f;
    private float chargeTimer = 0f;
    private bool isCharging = false;

    [Header("Referências")]
    public Camera fpsCam;
    public TextMeshProUGUI ammoText;
    public Slider chargeSlider; // Adicione no Inspector

    [Header("Recarga por olhar")]
    public float reloadLookDistance = 3f;
    public string reloadTag = "ReloadZone";

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        if (chargeSlider != null)
        {
            chargeSlider.gameObject.SetActive(false);
            chargeSlider.maxValue = chargeDuration;
            chargeSlider.value = 0f;
        }
    }

    void Update()
    {
        // Início do carregamento
        if (Input.GetButtonDown("Fire1") && canShoot && currentAmmo > 0)
        {
            isCharging = true;
            chargeTimer = 0f;
            if (chargeSlider != null)
                chargeSlider.gameObject.SetActive(true);
        }

        // Durante o carregamento
        if (Input.GetButton("Fire1") && isCharging)
        {
            chargeTimer += Time.deltaTime;
            if (chargeSlider != null)
                chargeSlider.value = chargeTimer;
        }

        // Soltou o botão = atira
        if (Input.GetButtonUp("Fire1") && isCharging)
        {
            float chargePercent = Mathf.Clamp01(chargeTimer / chargeDuration);
            int damageToDeal = Mathf.RoundToInt(maxDamage * chargePercent);

            Shoot(damageToDeal);
            isCharging = false;
            chargeTimer = 0f;
            if (chargeSlider != null)
            {
                chargeSlider.value = 0f;
                chargeSlider.gameObject.SetActive(false);
            }
        }

        // Verifica se está olhando para a zona de recarga
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryReload();
        }
    }

    void Shoot(int damage)
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
