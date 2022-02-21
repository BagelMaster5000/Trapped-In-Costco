using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] InputAction movementInput;
    [SerializeField] InputAction clickInput;
    [SerializeField] InputAction spinInput;

    public enum GameState {PLAYING, STARTMENU, PAUSEMENU, WINMENU};
    public GameState gameState = GameState.STARTMENU;

    Timer timer;

    [Header("Locations")]
    [SerializeField] Image background;
    [SerializeField] Location currentLocation;
    [SerializeField] Location[] allLocations;
    bool[] visitedLocations;

    [Header("Item Holding")]
    [SerializeField] Transform itemHoldLoc;
    [SerializeField] float baseSpinSpeed = 1;
    [SerializeField] float maxSpinSpeed = 25;
    float curSpinSpeed;
    [SerializeField] float spinSpeedStabilizationFactor = 20;
    [SerializeField] LayerMask itemLayer;
    [SerializeField] LayerMask backgroundLayer;
    [SerializeField] float throwForce = 10;
    GameObject heldItem = null;
    GameObject[] itemLocationFolders;
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
    public Action OnThrow;
    public Action OnPocket;
    public Action OnSmash;
    public Action OnSpin;
    public Action OnClap;
    public Action OnThumbsUp;
    public Action OnAngry;
           
    public Action OnGotCorrectItem;
    public Action OnGotWrongItem;

    #region Setup
    private void Start()
    {
        curSpinSpeed = baseSpinSpeed;

        InputsSetup();

        ItemsInLocationFoldersSetup();
        RefreshItems();
    }

    private void InputsSetup()
    {
        movementInput.performed += ctx => MoveInputRecieved(ctx.ReadValue<Vector2>());
        movementInput.Enable();
        clickInput.performed += ctx => ClickInputRecieved();
        clickInput.Enable();
        spinInput.performed += ctx => Spin();
        spinInput.Enable();

        OnGamePause = () =>
        {
            movementInput.Disable();
            clickInput.Disable();
            spinInput.Disable();
        };
        OnGameUnpause = () =>
        {
            movementInput.Enable();
            clickInput.Enable();
            spinInput.Enable();
        };
    }

    private void ItemsInLocationFoldersSetup()
    {
        itemLocationFolders = new GameObject[allLocations.Length];
        itemsInLocations = new GameObject[allLocations.Length][];
        for (int l = 0; l < allLocations.Length; l++)
        {
            GameObject curFolder = new GameObject("Location " + l + " Items");
            curFolder.transform.parent = transform;
            curFolder.SetActive(false);

            itemLocationFolders[l] = curFolder;


            itemsInLocations[l] = new GameObject[allLocations[l].itemsToSpawn.Length];
            for (int i = 0; i < itemsInLocations[l].Length; i++)
            {
                GameObject curItem = Instantiate(allLocations[l].itemsToSpawn[i].item.itemObject, curFolder.transform);
                curItem.transform.position = (Vector3)allLocations[l].itemsToSpawn[i].spawnLoc + Vector3.forward * 86;
                curItem.transform.localScale = Vector3.one * 5.35f;
                curItem.SetActive(true);

                itemsInLocations[l][i] = curItem;
            }
        }
    }
    #endregion

    public void StartGame() { } // Sets game state to playing
    public void SetPauseGame(bool setPause) { } // Sets game state to either playing or paused
    public void RestartGame() { }
    public void ExitGame() { }

    private void FixedUpdate()
    {
        if (heldItem != null)
        {
            SmoothMoveItemCloseToCamera();
            IdleItemRotation();
        }
    }
    private void SmoothMoveItemCloseToCamera()
    {
        heldItem.transform.localPosition /= 1.2f;
        heldItem.transform.rotation = Quaternion.Lerp(heldItem.transform.rotation, Quaternion.Euler(0f, heldItem.transform.eulerAngles.y, 0f), 0.1f);
    }
    private void IdleItemRotation()
    {
        heldItem.transform.RotateAround(heldItem.transform.position, Vector3.up, curSpinSpeed);
        if (curSpinSpeed > baseSpinSpeed)
        {
            curSpinSpeed = Mathf.Lerp(curSpinSpeed, baseSpinSpeed, 1 / spinSpeedStabilizationFactor);
            if (curSpinSpeed < baseSpinSpeed + 0.1f) curSpinSpeed = baseSpinSpeed;
        }
    }

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
        for (int l = 0; l < allLocations.Length; l++)
        {
            itemLocationFolders[l].SetActive(currentLocation.index == l);
        }
    }

    void ClickInputRecieved()
    {
        if (heldItem == null)
            Pickup();
        else
            Throw();
    }
    void Pickup()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.red, 100);

        if (Physics.Raycast(ray, out RaycastHit hit, 500, itemLayer))
        {
            heldItem = hit.transform.gameObject;
            heldItem.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            heldItem.GetComponent<Rigidbody>().velocity = Vector3.zero;
            hit.transform.parent = itemHoldLoc;
        }

        OnPickup?.Invoke(heldItem, "Test Name");
    }
    void Throw()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.DrawLine(Camera.main.transform.position, Mouse.current.position.ReadValue(), Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, 500, backgroundLayer))
        {
            heldItem.transform.parent = itemLocationFolders[currentLocation.index].transform;

            float spinForceMultiplier = Mathf.Clamp(curSpinSpeed / baseSpinSpeed, 1, 2);
            heldItem.GetComponent<Rigidbody>().AddForce(ray.direction * throwForce * spinForceMultiplier, ForceMode.Impulse);

            heldItem = null;
        }

        OnThrow?.Invoke();
    }
    void Pocket() { }
    void Smash() { }
    void Spin()
    {
        if (heldItem == null) return;

        curSpinSpeed = maxSpinSpeed;
    }

    void Clap() { }
    void ThumbsUp() { }
    void Angry() { }
}
