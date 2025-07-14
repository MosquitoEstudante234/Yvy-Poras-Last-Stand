using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCDialogue : MonoBehaviour
{
    [Header("Referências")]
    public TextMeshProUGUI dialogueText;

    [Header("Falas Aleatórias ao Abrir o Canvas")]
    [TextArea] public List<string> randomDialogues;

    [Header("Falas Específicas por Botão")]
    public List<ButtonDialogue> buttonDialogues;

    [Header("Configuração de Animação")]
    public float typingSpeed = 0.02f;

    private Coroutine typingCoroutine;

    void OnEnable()
    {
        if (dialogueText != null && randomDialogues.Count > 0)
        {
            int index = Random.Range(0, randomDialogues.Count);
            StartTyping(randomDialogues[index]);
        }
    }

    void OnDisable()
    {
        if (dialogueText != null)
            dialogueText.text = "";
    }

    public void SpeakSpecific(string key)
    {
        if (dialogueText == null) return;

        foreach (var item in buttonDialogues)
        {
            if (item.key == key && item.phrases.Count > 0)
            {
                int index = Random.Range(0, item.phrases.Count);
                StartTyping(item.phrases[index]);
                return;
            }
        }
    }

    void StartTyping(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}

[System.Serializable]
public class ButtonDialogue
{
    public string key;
    [TextArea] public List<string> phrases;
}
