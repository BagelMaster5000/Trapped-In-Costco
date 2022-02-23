public class PauseMenu : Menu
{
    public void ContinueGame() => GameController.staticReference.SetPauseGame(false);

    public void RestartGame() => GameController.staticReference.RestartGame();

    public void ExitGame() => GameController.staticReference.ExitGame();
}
