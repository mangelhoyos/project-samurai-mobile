using System;
using UnityEngine;

[Serializable]
public struct WordComposition
{
    [Tooltip("Cantidad de palabras del JSON FÁCIL")]
    public int EasyCount;

    [Tooltip("Cantidad de palabras del JSON MEDIO")]
    public int MediumCount;

    [Tooltip("Cantidad de palabras del JSON DIFÍCIL")]
    public int HardCount;

    // Helper para saber el total (opcional, útil para debug)
    public int TotalWords => EasyCount + MediumCount + HardCount;
}

[CreateAssetMenu(fileName = "NewMatchSettings", menuName = "WordSearch/Match Settings")]
public class MatchSettingsSO : ScriptableObject
{
    [Header("Configuración por Dificultad de Partida")]

    [Tooltip("Composición para cuando el usuario elige 'Fácil'")]
    public WordComposition EasyModeSettings;

    [Tooltip("Composición para cuando el usuario elige 'Medio'")]
    public WordComposition MediumModeSettings;

    [Tooltip("Composición para cuando el usuario elige 'Difícil'")]
    public WordComposition HardModeSettings;

    /// <summary>
    /// Función helper para obtener la configuración correcta según el enum
    /// </summary>
    public WordComposition GetComposition(WsDifficulty difficulty)
    {
        switch (difficulty)
        {
            case WsDifficulty.Easy: return EasyModeSettings;
            case WsDifficulty.Medium: return MediumModeSettings;
            case WsDifficulty.Hard: return HardModeSettings;
            default: return EasyModeSettings;
        }
    }
}
