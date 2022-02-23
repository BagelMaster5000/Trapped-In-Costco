public class PauseMenu : Menu
{
    public override void Start()
    {
        base.Start();

        GameController.staticReference.OnGamePause += Appear;
        GameController.staticReference.OnGameUnpause += Disappear;
    }

    public void ContinueGame() => GameController.staticReference.SetPauseGame(false);

    public void RestartGame() => GameController.staticReference.RestartGame();

    public void ExitGame() => GameController.staticReference.ExitGame();
}
