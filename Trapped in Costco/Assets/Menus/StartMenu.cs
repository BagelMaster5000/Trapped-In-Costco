public class StartMenu : Menu
{
    private void Awake()
    {
        
    }

    public void StartGame()
    {
        GameController.staticReference.StartGame();

        Disappear();
    }

    public void ExitGame() => GameController.staticReference.ExitGame();
}
