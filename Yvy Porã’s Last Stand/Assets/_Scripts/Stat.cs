// 28/10/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEngine; // Added namespace for Debug

[Serializable]
public class Stat
{
    public float baseValue; // Valor base do atributo
    public float currentValue; // Valor atual do atributo (após modificadores)

    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
        this.currentValue = baseValue; // Inicializa o valor atual com o valor base
    }

    // Adiciona um valor ao atributo
    public void Add(float value)
    {
        currentValue += value;
    }

    // Multiplica o atributo por um valor
    public void Multiply(float multiplier)
    {
        currentValue *= multiplier;
    }

    // Reseta o atributo para o valor base
    public void Reset()
    {
        currentValue = baseValue;
    }

    // Adiciona uma validação para valores negativos
    public void AddWithValidation(float value)
    {
        if (currentValue + value < 0)
        {
            Debug.LogWarning($"Tentativa de atribuir um valor negativo ao atributo. Valor atual: {currentValue}, Valor a adicionar: {value}");
            return;
        }
        currentValue += value;
    }

    // Multiplica com validação para evitar valores negativos
    public void MultiplyWithValidation(float multiplier)
    {
        if (currentValue * multiplier < 0)
        {
            Debug.LogWarning($"Tentativa de multiplicar o atributo por um valor que resulta em negativo. Valor atual: {currentValue}, Multiplicador: {multiplier}");
            return;
        }
        currentValue *= multiplier;
    }
}