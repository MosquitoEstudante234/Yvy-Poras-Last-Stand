using UnityEngine;

public class TestAnimatorSpeed : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Testa manualmente setando Speed
        float testSpeed = Input.GetKey(KeyCode.W) ? 5f : 0f;

        Debug.Log($"[TEST] Setando Speed para: {testSpeed}");
        animator.SetFloat("Speed", testSpeed); // Usa string diretamente

        float currentSpeed = animator.GetFloat("Speed");
        Debug.Log($"[TEST] Speed atual no Animator: {currentSpeed}");
    }
}