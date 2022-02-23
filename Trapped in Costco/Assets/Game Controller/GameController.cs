using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController staticReference;

    public enum GameState { PLAYING, STARTMENU, PAUSEMENU, WINMENU };
    public GameState gameState = GameState.STARTMENU;

    [Header("Controls")]
    [SerializeField] InputAction movementInput;
    [SerializeField] InputAction clickInput;

    [SerializeField] InputAction spinInput;
    [SerializeField] InputAction pocketInput;
    [SerializeField] InputAction smashInput;

    [SerializeField] InputAction clapInput;
    [SerializeField] InputAction thumbsUpInput;
    [SerializeField] InputAction angryInput;

    [SerializeField] InputAction pauseInput;

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

    [Header("Shopping List")]
    [SerializeField] int numShoppingListItems = 4;
    [SerializeField] Item[] availableItems;
    Item[] shoppingListItems;
    bool[] shoppingListCompletion;
    [SerializeField] TextMeshProUGUI[] shoppingListTexts;
    [SerializeField] Animator[] shoppingListAnimators;
    [SerializeField] Transform shoppingCartStorage;

    [Header("Blockades and Free Samples")]
    [SerializeField] GameObject blockadePrefab;
    int numBlockades = 1;
    bool[] blockedLocations;
    [SerializeField] GameObject freeSamplePrefab;
    int numFreeSamples = 1;
    bool[] freeSampleLocations;
    int buttonMashesToEscapeFreeSamples = 7;
    int remainingButtonMashes = -1;
    float afterFreeSampleKillMovementDisableTime = 0.5f;
    float afterFreeSampleKillMovementReenableTime;
    bool[] curBlockedDirections = { false, false, false, false }; // up, right, down, left


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
    private void Awake()
    {
        staticReference = this;
    }

    private void Start()
    {
        curSpinSpeed = baseSpinSpeed;

        InputsSetup();

        ShoppingListSetup();

        StartCoroutine(ContinuousItemLerpingAndRotation());
        ItemsInLocationFoldersSetup();
        RefreshItemsAtCurrentLocation();

        BlockadesSetup();
        FreeSamplesSetup();
    }

    private void InputsSetup()
    {
        movementInput.performed += ctx => MoveInputRecieved(ctx.ReadValue<Vector2>());
        movementInput.Enable();
        clickInput.performed += ctx => ClickInputRecieved();
        clickInput.Enable();

        spinInput.performed += ctx => Spin();
        spinInput.Enable();
        pocketInput.performed += ctx => Pocket();
        pocketInput.Enable();
        smashInput.performed += ctx => Smash();
        smashInput.Enable();

        clapInput.performed += ctx => Clap();
        clapInput.Enable();
        thumbsUpInput.performed += ctx => ThumbsUp();
        thumbsUpInput.Enable();
        angryInput.performed += ctx => Angry();
        angryInput.Enable();

        pauseInput.performed += ctx =>
        {
            if (gameState == GameState.PAUSEMENU)
                SetPauseGame(false);
            else if (gameState == GameState.PLAYING)
                SetPauseGame(true);
        };
        pauseInput.Enable();

        OnGamePause = () =>
        {
            movementInput.Disable();
            clickInput.Disable();

            spinInput.Disable();
            spinInput.Disable();
            pocketInput.Disable();
            smashInput.Disable();

            clapInput.Disable();
            thumbsUpInput.Disable();
            angryInput.Disable();
        };
        OnGameUnpause = () =>
        {
            movementInput.Enable();
            clickInput.Enable();

            spinInput.Enable();
            spinInput.Enable();
            pocketInput.Enable();
            smashInput.Enable();

            clapInput.Enable();
            thumbsUpInput.Enable();
            angryInput.Enable();
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
                curItem.name = allLocations[l].itemsToSpawn[i].item.itemName;
                curItem.SetActive(true);

                itemsInLocations[l][i] = curItem;
            }
        }
    }

    private void ShoppingListSetup()
    {
        if (availableItems.Length < numShoppingListItems)
            Debug.LogError("Not enough available items to populate shopping list");
        else
        {
            List<Item> tempItems = new List<Item>();
            for (int i = 0; i < availableItems.Length; i++)
                tempItems.Add(availableItems[i]);

            shoppingListItems = new Item[numShoppingListItems];
            shoppingListCompletion = new bool[numShoppingListItems];
            for (int n = 0; n < numShoppingListItems; n++)
            {
                int randomIndex = UnityEngine.Random.Range(0, tempItems.Count);
                Item randomItem = tempItems[randomIndex];
                tempItems.RemoveAt(randomIndex);

                shoppingListItems[n] = randomItem;
                shoppingListCompletion[n] = false;
            }
        }

        RefreshShoppingListTexts();
    }

    private void BlockadesSetup()
    {
        if (allLocations.Length < numBlockades)
            Debug.LogError("Not enough locations to populate for number of blockades");
        else
        {
            List<Location> tempLocations = new List<Location>();
            for (int i = 1; i < allLocations.Length; i++)
                tempLocations.Add(allLocations[i]);

            blockedLocations = new bool[allLocations.Length];
            for (int b = 0; b < numBlockades; b++)
            {
                int randomIndex = UnityEngine.Random.Range(0, tempLocations.Count);
                Location randomLocation = tempLocations[randomIndex];
                tempLocations.RemoveAt(randomIndex);

                Instantiate(blockadePrefab, itemLocationFolders[randomLocation.index].transform);
                blockedLocations[randomLocation.index] = true;
            }
        }
    }
    private void FreeSamplesSetup()
    {
        if (allLocations.Length < numFreeSamples + numBlockades)
            Debug.LogError("Not enough locations to populate for number of free sample stands");
        else
        {
            List<Location> tempLocations = new List<Location>();
            for (int i = 1; i < allLocations.Length; i++)
            {
                if (!blockedLocations[i])
                    tempLocations.Add(allLocations[i]);
            }

            freeSampleLocations = new bool[allLocations.Length];
            for (int b = 0; b < numFreeSamples; b++)
            {
                int randomIndex = UnityEngine.Random.Range(0, tempLocations.Count);
                Location randomLocation = tempLocations[randomIndex];
                tempLocations.RemoveAt(randomIndex);

                Instantiate(freeSamplePrefab, itemLocationFolders[randomLocation.index].transform);
                freeSampleLocations[randomLocation.index] = true;
            }
        }
    }
    #endregion

    public void StartGame()
    {
        gameState = GameState.PLAYING;

        OnGameStart?.Invoke();
    }
    public void SetPauseGame(bool setPause)
    {
        if (setPause)
        {
            gameState = GameState.PAUSEMENU;

            OnGamePause?.Invoke();
        }
        else
        {
            gameState = GameState.PLAYING;

            OnGameUnpause?.Invoke();
        }
    }
    public void RestartGame() { }
    public void ExitGame() { }

    #region Movement
    void MoveInputRecieved(Vector2 input)
    {
        if (freeSampleLocations[currentLocation.index])
        {
            remainingButtonMashes--;
            Transform currentLocationFolder = itemLocationFolders[currentLocation.index].transform;
            FreeSamplesStand freeSamplesStand = currentLocationFolder.GetComponentInChildren<FreeSamplesStand>();
            freeSamplesStand.Attacked();

            if (remainingButtonMashes <= 0)
            {
                ClearBlockedDirections();
                freeSamplesStand.Killed();

                afterFreeSampleKillMovementReenableTime = Time.time + afterFreeSampleKillMovementDisableTime;
            }
        }

        if (Time.time < afterFreeSampleKillMovementReenableTime) return;

        if (input.y > 0.8f) MoveUp();
        else if (input.x > 0.8f) MoveRight();
        else if (input.y < -0.8f) MoveDown();
        else if (input.x < -0.8f) MoveLeft();
    }
    void MoveUp()
    {
        if (currentLocation.upLocation != null && !curBlockedDirections[0])
        {
            Location previousLocation = currentLocation;
            currentLocation = currentLocation.upLocation;
            ArriveAtLocation(previousLocation);
        }

        OnMoveUp?.Invoke();
    }
    void MoveRight()
    {
        if (currentLocation.rightLocation != null && !curBlockedDirections[1])
        {
            Location previousLocation = currentLocation;
            currentLocation = currentLocation.rightLocation;
            ArriveAtLocation(previousLocation);
        }

        OnMoveRight?.Invoke();
    }
    void MoveDown()
    {
        if (currentLocation.downLocation != null && !curBlockedDirections[2])
        {
            Location previousLocation = currentLocation;
            currentLocation = currentLocation.downLocation;
            ArriveAtLocation(previousLocation);
        }

        OnMoveDown?.Invoke();
    }
    void MoveLeft()
    {
        if (currentLocation.leftLocation != null && !curBlockedDirections[3])
        {
            Location previousLocation = currentLocation;
            currentLocation = currentLocation.leftLocation;
            ArriveAtLocation(previousLocation);
        }

        OnMoveLeft?.Invoke();
    }
    #endregion

    #region Arriving at Location
    void ArriveAtLocation(Location previousLocation)
    {
        DestroyObstaclesFromPreviousLocation(previousLocation);
        RefreshBlockedDirections(previousLocation);

        background.sprite = currentLocation.background;
        RefreshItemsAtCurrentLocation();

        NewLocationQuip();

        OnArrivedAtLocation?.Invoke(currentLocation);
    }

    private void DestroyObstaclesFromPreviousLocation(Location previousLocation)
    {
        Transform previousLocationFolder = itemLocationFolders[previousLocation.index].transform;

        EmployeeBlockade employeeBlockade = previousLocationFolder.GetComponentInChildren<EmployeeBlockade>();
        if (employeeBlockade != null && employeeBlockade.ReadyToBeDestroyed())
            employeeBlockade.gameObject.SetActive(false);

        FreeSamplesStand freeSamplesStand = previousLocationFolder.GetComponentInChildren<FreeSamplesStand>();
        if (freeSamplesStand != null && freeSamplesStand.ReadyToBeDestroyed())
            freeSamplesStand.gameObject.SetActive(false);
    }
    private void RefreshBlockedDirections(Location previousLocation)
    {
        if (blockedLocations[currentLocation.index])
        {
            curBlockedDirections[0] = previousLocation != currentLocation.upLocation;
            curBlockedDirections[1] = previousLocation != currentLocation.rightLocation;
            curBlockedDirections[2] = previousLocation != currentLocation.downLocation;
            curBlockedDirections[3] = previousLocation != currentLocation.leftLocation;
        }
        else if (freeSampleLocations[currentLocation.index])
        {
            remainingButtonMashes = buttonMashesToEscapeFreeSamples;

            for (int d = 0; d < curBlockedDirections.Length; d++)
                curBlockedDirections[d] = true;
        }
        else
        {
            for (int d = 0; d < curBlockedDirections.Length; d++)
                curBlockedDirections[d] = false;
        }
    }
    public void ClearBlockedDirections()
    {
        blockedLocations[currentLocation.index] = false;
        freeSampleLocations[currentLocation.index] = false;

        for (int d = 0; d < curBlockedDirections.Length; d++)
            curBlockedDirections[d] = false;
    }

    void RefreshItemsAtCurrentLocation()
    {
        for (int l = 0; l < allLocations.Length; l++)
        {
            itemLocationFolders[l].SetActive(currentLocation.index == l);
        }
    }

    private void NewLocationQuip()
    {
        if (currentLocation.allQuips.Length > 0 && UnityEngine.Random.Range(0.0f, 1.0f) < currentLocation.quipChance)
        {
            string randomQuip = currentLocation.allQuips[UnityEngine.Random.Range(0, currentLocation.allQuips.Length)];
            OnQuip?.Invoke(randomQuip);
        }
    }
    #endregion


    #region Shopping List
    void RefreshShoppingListTexts()
    {
        for (int t = 0; t < shoppingListTexts.Length; t++)
        {
            if (t < numShoppingListItems)
            {
                shoppingListTexts[t].text = shoppingListItems[t].itemName;
                shoppingListTexts[t].color = shoppingListCompletion[t] ? Color.green : Color.white;
            }
            else
                shoppingListTexts[t].text = "";
        }
    }
    void PlayShoppingListItemCheckAnimation() { } // Play check animation
    #endregion


    #region Item
    IEnumerator ContinuousItemLerpingAndRotation()
    {
        while (gameState != GameState.WINMENU)
        {
            if (heldItem != null)
            {
                SmoothMoveItemCloseToCamera();
                IdleItemRotation();
            }

            yield return new WaitForFixedUpdate();
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
            if (curSpinSpeed < baseSpinSpeed + 0.1f)
            {
                curSpinSpeed = baseSpinSpeed;
            }
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
        //Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.red, 100);

        if (Physics.Raycast(ray, out RaycastHit hit, 500, itemLayer))
        {
            if (!hit.transform.CompareTag("CostcoProduct")) return;

            heldItem = hit.transform.gameObject;
            heldItem.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            heldItem.GetComponent<Rigidbody>().velocity = Vector3.zero;
            hit.transform.parent = itemHoldLoc;

            OnPickup?.Invoke(heldItem, heldItem.name);
        }
        else
            OnPickup?.Invoke(null, "");
    }
    void Throw()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        //Debug.DrawLine(Camera.main.transform.position, Mouse.current.position.ReadValue(), Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, 500, backgroundLayer))
        {
            heldItem.transform.parent = itemLocationFolders[currentLocation.index].transform;

            float spinForceMultiplier = Mathf.Clamp(curSpinSpeed / baseSpinSpeed, 1, 2);
            heldItem.GetComponent<Rigidbody>().AddForce(ray.direction * throwForce * spinForceMultiplier, ForceMode.Impulse);

            heldItem = null;
        }

        OnThrow?.Invoke();
    }
    void Pocket()
    {
        if (heldItem == null) return;

        bool itemFoundInShoppingList = false;
        string itemName = heldItem.name;
        for (int s = 0; s < shoppingListItems.Length && !itemFoundInShoppingList; s++)
        {
            if (!shoppingListCompletion[s] && itemName == shoppingListItems[s].itemName)
            {
                shoppingListCompletion[s] = true;
                RefreshShoppingListTexts();
                // TODO Play check animation for collected item

                print("got correct item");
                OnGotCorrectItem?.Invoke();

                itemFoundInShoppingList = true;
            }
        }
        if (!itemFoundInShoppingList)
        {
            print("got wrong item");
            OnGotWrongItem?.Invoke();
        }

        OnPocket?.Invoke();

        heldItem.transform.parent = shoppingCartStorage;
        heldItem.SetActive(false);
        heldItem = null;
    }
    void Smash()
    {
        if (heldItem == null) return;

        print("smashed item");
        OnSmash?.Invoke();

        heldItem.transform.parent = shoppingCartStorage;
        heldItem.SetActive(false);
        heldItem = null;
    }
    void Spin()
    {
        if (heldItem == null) return;

        curSpinSpeed = maxSpinSpeed;
    }
    #endregion

    #region Emotes
    void Clap() { }
    void ThumbsUp() { }
    void Angry() { }
    #endregion
}