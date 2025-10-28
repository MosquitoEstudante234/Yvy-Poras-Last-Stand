// 28/10/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;

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
}