using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TextHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    private TextMeshProUGUI text;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = normalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (text != null)
        {
            text.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (text != null)
        {
            text.color = normalColor;
        }
    }
}