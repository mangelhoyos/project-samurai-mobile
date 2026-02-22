using UnityEngine;

public class PlayController : MonoBehaviour
{
    private MatchSettingsModel model;
    private void Awake() => model = GetComponentInParent<MatchSettingsModel>();
    public void PlayMatch()
    {
        model.GetWordList();
    }
}
