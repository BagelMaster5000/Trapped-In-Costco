public class StartMenu : Menu
{
    public void StartGame() => GameController.staticReference.StartGame();

    public void ExitGame() => GameController.staticReference.ExitGame();
}
