using UnityEngine;

public class CathegoryController : MonoBehaviour
{
    [SerializeField] private WsCategory category;

    private MatchSettingsModel model;

    private void Awake() => model = GetComponentInParent<MatchSettingsModel>();
    
    public void SelectCathegory()
    {
        model.SelectCathegory(category);
    }
}
