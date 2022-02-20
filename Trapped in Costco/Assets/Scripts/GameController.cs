using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public enum GameState {PLAYING, STARTMENU, PAUSEMENU, WINMENU};
    public GameState gameState = GameState.STARTMENU;
    [SerializeField] InputAction movementInput;

    Timer timer;

    [SerializeField] Image background;
    [SerializeField] Location currentLocation;
    [SerializeField] Location[] allLocations;
    bool[] visitedLocations;

    GameObject heldItem = null;
    GameObject[][] itemsInLocations;
    Item[] allItems;
    int shoppingListLength;
    Item[] shoppingListItems;
    bool[] shoppingListCompletion;
    TextMeshProUGUI[] shoppingListTexts;
    Animator[] shoppingListAnimators;

    public Action OnGameStart;
    public Action OnGamePause;
    public Action OnGameUnpause;
    public Action OnGameWin;
    public Action OnGameRestart;
    public Action OnGameExit;
           
    public Action<Location> OnArrivedAtLocation;
    public Action<string> OnQuip;
           
    public Action OnMoveUp;
    public Action OnMoveRight;
    public Action OnMoveDown;
    public Action OnMoveLeft;
           
    public Action<GameObject, string> OnPickup;
    public Action OnPocket;
    public Action OnSmash;
    public Action OnSpin;
    public Action OnClap;
    public Action OnThumbsUp;
    public Action OnAngry;
           
    public Action OnGotCorrectItem;
    public Action OnGotWrongItem;


    private void Start()
    {
        movementInput.performed += ctx => MoveInputRecieved(ctx.ReadValue<Vector2>());
        movementInput.Enable();

        OnGamePause = () => { movementInput.Disable(); };
        OnGameUnpause = () => { movementInput.Enable(); };

        RefreshItems();
    }

    public void StartGame() { } // Sets game state to playing
    public void SetPauseGame(bool setPause) { } // Sets game state to either playing or paused
    public void RestartGame() { }
    public void ExitGame() { }

    /* Sets item texts in shopping list
     * Plays check animations for shopping list items that were found between now and the last refresh
     */
    void RefreshShoppingList() { }

    #region Movement
    void MoveInputRecieved(Vector2 input)
    {
        if (input.y > 0.8f) MoveUp();
        else if (input.x > 0.8f) MoveRight();
        else if (input.y < -0.8f) MoveDown();
        else if (input.x < -0.8f) MoveLeft();
    }
    void MoveUp()
    {
        if (currentLocation.upLocation != null)
        {
            currentLocation = currentLocation.upLocation;
            ArriveAtLocation();
        }

        OnMoveUp?.Invoke();
    }
    void MoveRight()
    {
        if (currentLocation.rightLocation != null)
        {
            currentLocation = currentLocation.rightLocation;
            ArriveAtLocation();
        }

        OnMoveRight?.Invoke();
    }
    void MoveDown()
    {
        if (currentLocation.downLocation != null)
        {
            currentLocation = currentLocation.downLocation;
            ArriveAtLocation();
        }

        OnMoveDown?.Invoke();
    }
    void MoveLeft()
    {
        if (currentLocation.leftLocation != null)
        {
            currentLocation = currentLocation.leftLocation;
            ArriveAtLocation();
        }

        OnMoveLeft?.Invoke();
    }
    #endregion

    void ArriveAtLocation()
    {
        background.sprite = currentLocation.background;
        RefreshItems();

        OnArrivedAtLocation?.Invoke(currentLocation);
    }
    void RefreshItems()
    {
        // Show/hide items depending on current location
    }

    void Pickup()
    {
        // Raycast to check for item collision
        // Set held item

        // On pickup action
    }
    void Pocket() { }
    void Smash() { }
    void Spin() { }

    void Clap() { }
    void ThumbsUp() { }
    void Angry() { }
}
