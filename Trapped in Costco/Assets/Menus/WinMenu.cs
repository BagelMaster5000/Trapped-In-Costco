public class WinMenu : Menu
{
    public void RestartGame() => GameController.staticReference.RestartGame();

    public void ExitGame() => GameController.staticReference.ExitGame();
}
