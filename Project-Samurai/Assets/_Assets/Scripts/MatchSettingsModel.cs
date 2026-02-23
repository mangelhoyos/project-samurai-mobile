using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchSettingsModel : MonoBehaviour
{
    [SerializeField] private MatchSettingsSO matchSettings;


    public event Action WordListUpdated;
    public WsLanguage Language { get; private set; } = WsLanguage.Spanish;
    public WsCategory Category { get; private set; } = WsCategory.None;
    public WsDifficulty Difficulty { get; private set; } = WsDifficulty.None;
    public List<string> Words { get; private set; } = new();

    public async void SelectCathegory(WsCategory category)
    {
        try
        {
            Debug.Log($"Start Loading JSONS with words for {category}");
            Words.Clear();

            Category = category;
            await WordDataManager.LoadSessionDataAsync(category, Language);

            Debug.Log($"JSONS Loaded");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public void SelectDifficulty(WsDifficulty difficulty)
    {
        Words.Clear();
        Difficulty = difficulty;
    }

    public void GetWordList()
    {
        if (Category == WsCategory.None || Difficulty == WsDifficulty.None)
        {
            Debug.LogWarning($"[MatchSettingsModel] Category({Category}) or Difficulty({Difficulty}) not setted.");
            return;
        }

        Words = WordDataManager.GetWordsForMatch(Difficulty, matchSettings);
        WordListUpdated?.Invoke();
        Debug.Log("Contenido de la lista: " + string.Join(", ", Words));
    }
}
