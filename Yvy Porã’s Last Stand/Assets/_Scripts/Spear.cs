using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Spear : MonoBehaviour
{
    [Header("Configuração do Ataque")]
    public int damage = 25;
    public float cooldownTime = 0.4f;
    public float attackRange = 4f;
    public LayerMask enemyLayer;

    [Header("Cooldown UI")]
    public Slider cooldownSlider;
    public CanvasGroup cooldownCanvasGroup;
    public float fadeSpeed = 5f;

    private bool canAttack = true;
    private bool isFadingIn = false;
    private bool isFadingOut = false;
    private float currentCooldown = 0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            Attack();
        }

        if (!canAttack)
        {
            currentCooldown += Time.deltaTime;
            cooldownSlider.value = currentCooldown;

            if (isFadingIn)
            {
                if (cooldownCanvasGroup.alpha < 1)
                    cooldownCanvasGroup.alpha = Mathf.Lerp(cooldownCanvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
            }
        }
        else
        {
            if (isFadingOut)
            {
                if (cooldownCanvasGroup.alpha > 0.01f)
                {
                    cooldownCanvasGroup.alpha = Mathf.Lerp(cooldownCanvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
                }
                else
                {
                    cooldownCanvasGroup.alpha = 0f;
                    cooldownSlider.gameObject.SetActive(false);
                    isFadingOut = false;
                }
            }
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
                    int dealtDamage = firstHit ? damage : damage / 2;
                    enemy.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, dealtDamage);
                    firstHit = false;
                }
            }
        }
        AudioManager.instance.Play("Spear");
        StartCoroutine(StartCooldown());
    }

    IEnumerator StartCooldown()
    {
        canAttack = false;
        isFadingIn = true;
        isFadingOut = false;

        cooldownSlider.gameObject.SetActive(true);
        cooldownCanvasGroup.alpha = 0f;

        cooldownSlider.maxValue = cooldownTime;
        cooldownSlider.value = 0f;
        currentCooldown = 0f;

        yield return new WaitForSeconds(cooldownTime);

        canAttack = true;
        isFadingIn = false;
        isFadingOut = true;
    }
}
