public class WinMenu : Menu
{
    private void Awake()
    {
        GameController.staticReference.OnGameWin += Appear;
    }

    public override void Start()
    {
        base.Start();
    }

    public void RestartGame() => GameController.staticReference.RestartGame();

    public void ExitGame() => GameController.staticReference.ExitGame();
}
