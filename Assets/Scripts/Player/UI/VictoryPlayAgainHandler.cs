/// <summary>
/// PLAY AGAIN button on VictoryScene — replays the level that was just completed.
/// </summary>
public class VictoryPlayAgainHandler : UnityEngine.MonoBehaviour
{
    public void OnClick()
    {
        int level = LevelProgress.CurrentLevel;
        SceneFader.LoadScene(LevelCatalog.GetGameplayScene(level));
    }
}
