using TMPro;
using UnityEngine;

public class WinMenu : Menu
{
    [Header("Win Counters")]
    [SerializeField] TextMeshProUGUI timeCounter;
    [SerializeField] TextMeshProUGUI mistakesCounter;

    private void Awake()
    {
        GameController.staticReference.OnGameWin += Appear;
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Appear()
    {
        base.Appear();
        timeCounter.text = Timer.staticReference.GetTimeFormatted(timeCounter.fontSize - 2.5f);
        mistakesCounter.text = GameController.staticReference.GetNumberOfIncorrectItemsAddedToCart().ToString();
    }

    public void RestartGame() => GameController.staticReference.RestartGame();

    public void ExitGame() => GameController.staticReference.ExitGame();
}
