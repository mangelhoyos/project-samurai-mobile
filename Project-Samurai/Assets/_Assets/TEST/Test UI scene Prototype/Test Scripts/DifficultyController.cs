using UnityEngine;

public class DifficultyController : MonoBehaviour
{
    [SerializeField] private WsDifficulty difficulty;

    private MatchSettingsModel model;

    private void Awake() => model = GetComponentInParent<MatchSettingsModel>();

    public void SelectDifficulty()
    {
        model.SelectDifficulty(difficulty);
    }
}
