using UnityEngine;

public class FloatingRotatingItem : MonoBehaviour
{
    [Header("Rota��o")]
    public float rotationSpeed = 30f; // graus por segundo

    [Header("Oscila��o Vertical")]
    public float floatAmplitude = 0.5f; // altura do sobe/desce
    public float floatFrequency = 1f;   // velocidade do sobe/desce

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Rota��o lenta no eixo Y
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Movimento vertical oscilante com seno
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
