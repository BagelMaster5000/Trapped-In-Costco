public class PauseMenu : Menu
{
    private void Awake()
    {
        GameController.staticReference.OnGamePause += Appear;
        GameController.staticReference.OnGameUnpause += Disappear;
    }

    public override void Start()
    {
        base.Start();
    }

    public void ContinueGame() => GameController.staticReference.SetPauseGame(false);

    public void RestartGame() => GameController.staticReference.RestartGame();

    public void ExitGame() => GameController.staticReference.ExitGame();
}
