using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TextHoverShake : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI textMesh;
    private bool isHovering = false;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        StartCoroutine(ShakeText());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    IEnumerator ShakeText()
    {
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;

        Vector3[][] copyOfVertices = new Vector3[0][];

        while (isHovering)
        {
            textMesh.ForceMeshUpdate();
            textInfo = textMesh.textInfo;

            if (copyOfVertices.Length < textInfo.meshInfo.Length)
            {
                copyOfVertices = new Vector3[textInfo.meshInfo.Length][];
            }

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

                if (copyOfVertices[materialIndex] == null || copyOfVertices[materialIndex].Length != sourceVertices.Length)
                    copyOfVertices[materialIndex] = new Vector3[sourceVertices.Length];

                System.Array.Copy(sourceVertices, copyOfVertices[materialIndex], sourceVertices.Length);

                Vector3 jitter = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
                for (int j = 0; j < 4; j++)
                {
                    sourceVertices[vertexIndex + j] += jitter;
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return new WaitForSeconds(0.03f);
        }

        // Resetar a malha
        textMesh.ForceMeshUpdate();
    }
}
