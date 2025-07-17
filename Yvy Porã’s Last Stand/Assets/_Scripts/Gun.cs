using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Cinemachine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("Munição")]
    public PlayerStats stats;
    public int maxAmmo = 10;
    private int currentAmmo;
    //berne, mudei o max ammo pra public só pra testar uns ngc, tava dando conflito em outros códigos por ele estar private

    [Header("Cooldown e Dano")]
    public float cooldownTime = 0.5f;
    private bool canShoot = true;
    public float range = 100f;
    public int maxDamage = 40;

    [Header("Carregamento")]
    public float chargeDuration = 1.5f;
    private float chargeTimer = 0f;
    private bool isCharging = false;

    [Header("Referências")]
    public Camera fpsCam;
    public TextMeshProUGUI ammoText;
    public Slider chargeSlider;

    [Header("Recarga por olhar")]
    public float reloadLookDistance = 3f;
    public string reloadTag = "ReloadZone";

    [Header("Cinemachine")]
    public CinemachineVirtualCamera virtualCamera;
    public float fovIncreaseAmount = 15f; // Quanto o FOV vai AUMENTAR
    public float fovTransitionTime = 0.5f;

    [Header("Tremor")]
    public float maxShakeAmplitude = 2f;
    public float maxShakeFrequency = 2f;

    private float originalFOV;
    private Tween fovTween;
    private CinemachineBasicMultiChannelPerlin noise;

    private CanvasGroup chargeSliderCanvasGroup;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        if (chargeSlider != null)
        {
            chargeSliderCanvasGroup = chargeSlider.GetComponent<CanvasGroup>();
            if (chargeSliderCanvasGroup == null)
            {
                chargeSliderCanvasGroup = chargeSlider.gameObject.AddComponent<CanvasGroup>();
            }
            chargeSlider.gameObject.SetActive(false);
            chargeSliderCanvasGroup.alpha = 0f;
            chargeSlider.maxValue = chargeDuration;
            chargeSlider.value = 0f;
        }

        if (virtualCamera != null)
        {
            originalFOV = virtualCamera.m_Lens.FieldOfView;
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && canShoot && currentAmmo > 0)
        {
            isCharging = true;
            chargeTimer = 0f;

            if (chargeSlider != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeInSlider());
            }

            StartFOVIncrease();
        }

        if (Input.GetButton("Fire1") && isCharging)
        {
            chargeTimer += Time.deltaTime;
            float chargePercent = Mathf.Clamp01(chargeTimer / chargeDuration);

            if (chargeSlider != null)
                chargeSlider.value = chargeTimer;

            ApplyShake(chargePercent);
        }

        if (Input.GetButtonUp("Fire1") && isCharging)
        {
            float chargePercent = Mathf.Clamp01(chargeTimer / chargeDuration);
            int damageToDeal = Mathf.RoundToInt(maxDamage * chargePercent);

            Shoot(damageToDeal);
            ResetChargingState();
        }

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

        int mask = ~LayerMask.GetMask("EnemyIgnore");

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit hit, range, mask))
        {
            Debug.Log("Acertou: " + hit.transform.name);

            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        Invoke(nameof(ResetCooldown), cooldownTime);
        AudioManager.instance.Play("Shoot");
    }

    void ResetCooldown()
    {
        canShoot = true;
    }

    void TryReload()
    {
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit hit, reloadLookDistance))
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

    void StartFOVIncrease()
    {
        if (virtualCamera == null) return;

        fovTween?.Kill();
        fovTween = DOTween.To(
            () => virtualCamera.m_Lens.FieldOfView,
            x => virtualCamera.m_Lens.FieldOfView = x,
            originalFOV + fovIncreaseAmount,
            chargeDuration
        ).SetEase(Ease.OutSine);
    }

    void ResetFOV()
    {
        if (virtualCamera == null) return;

        fovTween?.Kill();
        fovTween = DOTween.To(
            () => virtualCamera.m_Lens.FieldOfView,
            x => virtualCamera.m_Lens.FieldOfView = x,
            originalFOV,
            fovTransitionTime
        ).SetEase(Ease.InOutSine);
    }

    void ApplyShake(float chargePercent)
    {
        if (noise == null) return;

        noise.m_AmplitudeGain = maxShakeAmplitude * chargePercent;
        noise.m_FrequencyGain = maxShakeFrequency * chargePercent;
    }

    void StopShake()
    {
        if (noise == null) return;

        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
    }

    void ResetChargingState()
    {
        isCharging = false;
        chargeTimer = 0f;

        if (chargeSlider != null)
        {
            chargeSlider.value = 0f;
            StopAllCoroutines();
            StartCoroutine(FadeOutSlider());
        }

        ResetFOV();
        StopShake();
    }

    private IEnumerator FadeInSlider()
    {
        chargeSlider.gameObject.SetActive(true);

        Vector3 startPos = chargeSlider.transform.localPosition;
        Vector3 endPos = startPos + new Vector3(0, 30f, 0); // sobe 30 unidades no Y

        float duration = 0.3f;
        float elapsed = 0f;

        chargeSliderCanvasGroup.alpha = 0f;
        chargeSlider.transform.localPosition = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            chargeSliderCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            chargeSlider.transform.localPosition = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        chargeSliderCanvasGroup.alpha = 1f;
        chargeSlider.transform.localPosition = endPos;
    }

    private IEnumerator FadeOutSlider()
    {
        Vector3 startPos = chargeSlider.transform.localPosition;
        Vector3 endPos = startPos - new Vector3(0, 30f, 0);

        float duration = 0.3f;
        float elapsed = 0f;

        chargeSliderCanvasGroup.alpha = 1f;
        chargeSlider.transform.localPosition = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            chargeSliderCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            chargeSlider.transform.localPosition = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        chargeSliderCanvasGroup.alpha = 0f;
        chargeSlider.transform.localPosition = endPos;
        chargeSlider.gameObject.SetActive(false);
    }
}
