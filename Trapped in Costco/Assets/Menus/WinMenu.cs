public class WinMenu : Menu
{
    public override void Start()
    {
        base.Start();

        GameController.staticReference.OnGameWin += Appear;
    }

    public void RestartGame() => GameController.staticReference.RestartGame();

    public void ExitGame() => GameController.staticReference.ExitGame();
}
