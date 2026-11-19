using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private string firstLevelScene = "Level01";

    private void Start() => continueButton.interactable = SaveSystem.HasSave;

    public void NewGame()
    {
        SaveSystem.Delete();
        GameProgress.Reset();
        Flow.Go(firstLevelScene);
    }

    public void Continue()
    {
        GameProgress.Apply(SaveSystem.Load());
        Flow.Go(Mathf.Max(1, GameProgress.LevelIndex));
    }

    public void Quit() => Flow.Quit();
}