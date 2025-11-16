using UnityEngine;
using UnityEngine.Rendering;

public class DamageVignette : MonoBehaviour
{
    [Header("Post Processing")]
    [SerializeField] private Volume damageVolume;

    [Header("Configurações de Transição")]
    [SerializeField] private float maxWeight = 1f; // Peso máximo quando toma dano
    [SerializeField] private float fadeSpeed = 2f; // Velocidade que o efeito desaparece
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Curva de suavização

    private float currentWeight = 0f;
    private float targetWeight = 0f;

    void Start()
    {
        if (damageVolume != null)
        {
            damageVolume.weight = 0f;
        }
    }

    void Update()
    {
        if (damageVolume == null) return;

        // Interpola suavemente entre o peso atual e o alvo
        if (currentWeight > targetWeight)
        {
            currentWeight -= fadeSpeed * Time.deltaTime;
            currentWeight = Mathf.Max(currentWeight, targetWeight);

            // Aplica a curva de suavização
            float normalizedWeight = currentWeight / maxWeight;
            damageVolume.weight = fadeCurve.Evaluate(normalizedWeight) * currentWeight;
        }
    }

    // Chame este método quando o jogador tomar dano
    public void TriggerDamageEffect()
    {
        currentWeight = maxWeight;
        targetWeight = 0f;
    }

    // Versão com intensidade variável baseada no dano recebido
    public void TriggerDamageEffect(float damageAmount, float maxHealth)
    {
        float intensity = Mathf.Clamp01(damageAmount / (maxHealth * 0.3f)); // 30% da vida = efeito máximo
        currentWeight = maxWeight * intensity;
        targetWeight = 0f;
    }
}