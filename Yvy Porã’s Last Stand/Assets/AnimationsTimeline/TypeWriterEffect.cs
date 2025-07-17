using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    [TextArea] public string fullText;
    public float delayPerCharacter = 0.05f;

    private Coroutine typingCoroutine;

    private void OnEnable()
    {
        // Garante que comece o efeito toda vez que ativar
        if (textMeshPro != null)
        {
            typingCoroutine = StartCoroutine(ShowTextGradually());
        }
    }

    private void OnDisable()
    {
        // Cancela a coroutine se o objeto for desativado no meio
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    IEnumerator ShowTextGradually()
    {
        textMeshPro.text = "";
        for (int i = 0; i <= fullText.Length; i++)
        {
            textMeshPro.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(delayPerCharacter);
        }
    }
}
