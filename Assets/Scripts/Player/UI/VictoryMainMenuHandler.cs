/// <summary>
/// MAIN MENU button on VictoryScene — returns to StartScene.
/// </summary>
public class VictoryMainMenuHandler : UnityEngine.MonoBehaviour
{
    public void OnClick()
    {
        SceneFader.LoadScene("StartScene");
    }
}
