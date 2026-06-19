/// <summary>
/// Attached to the "PLAY LEVEL 2" button on the VictoryScene.
/// Advances level progress to 2 and loads the Level 2 gameplay scene.
/// </summary>
public class Level2ButtonHandler : UnityEngine.MonoBehaviour
{
    public void GoToLevel2()
    {
        LevelProgress.SetLevel(2);
        string scene = LevelCatalog.GetGameplayScene(2);
        SceneFader.LoadScene(scene);
    }
}
