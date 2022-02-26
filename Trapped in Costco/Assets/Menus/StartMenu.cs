public class StartMenu : Menu
{
    public void StartGame()
    {
        GameController.staticReference.StartGame();

        Disappear();
    }

    public void ExitGame() => GameController.staticReference.ExitGame();
}
