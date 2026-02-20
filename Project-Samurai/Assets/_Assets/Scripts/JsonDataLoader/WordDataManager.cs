using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Threading.Tasks;

public enum WsCategory
{
    Animals,
    Food,
    Travel,
    Science
}

public enum WsDifficulty
{
    Easy,
    Medium,
    Hard
}

public enum WsLanguage
{
    English,
    Spanish
}

[Serializable]
public class WordListWrapper
{
    public string[] words;
}

public static class WordDataManager
{
    private static Dictionary<WsDifficulty, string[]> _loadedWords = new Dictionary<WsDifficulty, string[]>(); //Test with profiler in case we don't need to keep words loaded
    private static Dictionary<WsDifficulty, List<int>> _shuffledIndices = new Dictionary<WsDifficulty, List<int>>();
    private static Dictionary<WsDifficulty, int> _cursors = new Dictionary<WsDifficulty, int>();

    public static async Task LoadSessionDataAsync(WsCategory category, WsLanguage language)
    {
        _loadedWords.Clear();
        _shuffledIndices.Clear();
        _cursors.Clear();

        try
        {
            await LoadSingleFileAsync(category, language, WsDifficulty.Easy);
            await LoadSingleFileAsync(category, language, WsDifficulty.Medium);
            await LoadSingleFileAsync(category, language, WsDifficulty.Hard);

            Debug.Log($"<color=green>Éxito:</color> Datos cargados desde Resources.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error fatal cargando sesión: {e.Message}");
            throw;
        }
    }

    private static async Task LoadSingleFileAsync(WsCategory cat, WsLanguage lang, WsDifficulty diff)
    {
        string fileName = $"{cat.ToString().ToUpper()}_{lang.ToString().ToUpper()}_{diff.ToString().ToUpper()}";
        string resourcePath = $"{cat.ToString()}/{fileName}";

        ResourceRequest request = Resources.LoadAsync<TextAsset>(resourcePath);

        while (!request.isDone)
        {
            await Task.Yield();
        }

        TextAsset textFile = request.asset as TextAsset;

        if (textFile == null)
        {
            throw new System.IO.FileNotFoundException($"No se encontró el archivo en Resources: {resourcePath}. Revisa el nombre o la carpeta.");
        }

        WordListWrapper data = JsonUtility.FromJson<WordListWrapper>(textFile.text);

        if (data == null || data.words == null || data.words.Length == 0)
            throw new System.Exception($"El JSON {fileName} está vacío o mal formateado.");

        _loadedWords[diff] = data.words;
        InitializeIndicesForDifficulty(diff, data.words.Length);
    }

    private static void InitializeIndicesForDifficulty(WsDifficulty diff, int count)
    {
        List<int> indices = Enumerable.Range(0, count).ToList();
        System.Random rng = new System.Random();
        int n = indices.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int value = indices[k];
            indices[k] = indices[n];
            indices[n] = value;
        }
        _shuffledIndices[diff] = indices;
        _cursors[diff] = 0;
    }

    public static List<string> GetWordsForMatch(WsDifficulty matchDifficulty, MatchSettingsSO settings)
    {
        List<string> matchWords = new List<string>();

        if (_loadedWords.Count < 3)
        {
            Debug.LogError("No se han cargado las palabras. Llama a LoadSessionDataAsync primero.");
            return matchWords;
        }

        if (settings == null)
        {
            Debug.LogError("Los settings son nulos.");
            return matchWords;
        }

        WordComposition composition = settings.GetComposition(matchDifficulty);

        matchWords.AddRange(GetNextWords(WsDifficulty.Easy, composition.EasyCount));
        matchWords.AddRange(GetNextWords(WsDifficulty.Medium, composition.MediumCount));
        matchWords.AddRange(GetNextWords(WsDifficulty.Hard, composition.HardCount));

        return matchWords;
    }

    private static List<string> GetNextWords(WsDifficulty diff, int amountNeeded)
    {
        List<string> result = new List<string>();
        var indices = _shuffledIndices[diff];
        var words = _loadedWords[diff];
        int cursor = _cursors[diff];

        for (int i = 0; i < amountNeeded; i++)
        {
            if (cursor >= indices.Count)
            {
                InitializeIndicesForDifficulty(diff, words.Length);
                indices = _shuffledIndices[diff];
                cursor = 0;
            }
            int wordIndex = indices[cursor];
            result.Add(words[wordIndex]);
            cursor++;
        }
        _cursors[diff] = cursor;
        return result;
    }

    //TODO save shuffle indices and cursor in player Prefs
    // Keep in mind that Jsons can be expanded
}

