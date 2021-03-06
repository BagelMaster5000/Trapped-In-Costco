using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

    [Header("Locations")]
    [SerializeField] Image background;
    [SerializeField] Location currentLocation;
    [SerializeField] Location[] allLocations;

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
    [SerializeField] Transform shoppingCartStorage;
    int numIncorrectItemsAddedToCart = 0;

    [Header("Blockades and Free Samples")]
    [SerializeField] GameObject blockadePrefab;
    [SerializeField] int numBlockades = 1;
    bool[] blockedLocations;
    [SerializeField] GameObject freeSamplePrefab;
    [SerializeField] int numFreeSamples = 1;
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

    public Action<bool> OnMove; // bool able to move
    public Action OnMoveUp;
    public Action OnMoveRight;
    public Action OnMoveDown;
    public Action OnMoveLeft;
    public Action OnTryMoveWhileFreeSamples;
    public Action OnTryExitBeforeShoppingListComplete;

    public Action<string> OnPickup;
    public Action OnThrow;
    public Action OnPocket;
    public Action OnSmash;
    public Action OnSpin;
    public Action OnClap;
    public Action OnThumbsUp;
    public Action OnAngry;

    public Action OnGotCorrectItem;
    public Action OnGotWrongItem;
    public Action OnShoppingListComplete;

    public Action OnBlockedByFreeSamples;
    public Action OnBlockedByMembershipEmployee;
    public Action<Location> OnClearedBlockage;
    public Action OnBlockedFromLeaving;
    public Action OnAbleToLeave;

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

        background.sprite = currentLocation.background;
        OnGameStart += () => { OnArrivedAtLocation?.Invoke(currentLocation); };

        StartCoroutine(ContinuousItemLerpingAndRotation());
        ItemsInLocationFoldersSetup();
        RefreshItemsAtCurrentLocation();

        BlockadesSetup();
        FreeSamplesSetup();

        // DEBUG complete shopping list at start of game
        //for (int s = 0; s < shoppingListCompletion.Length; s++)
        //    shoppingListCompletion[s] = true;
        //RefreshShoppingListTexts();
    }

    private void InputsSetup()
    {
        movementInput.performed += ctx => MoveInputRecieved(ctx.ReadValue<Vector2>());
        movementInput.Enable();
        clickInput.performed += ctx => ClickInputRecieved();
        clickInput.Enable();

        clapInput.performed += ctx => Clap();
        clapInput.Enable();
        thumbsUpInput.performed += ctx => ThumbsUp();
        thumbsUpInput.Enable();
        angryInput.performed += ctx => Angry();
        angryInput.Enable();

        spinInput.performed += ctx => Spin();
        spinInput.Enable();
        pocketInput.performed += ctx => Pocket();
        pocketInput.Enable();
        smashInput.performed += ctx => Smash();
        smashInput.Enable();

        pauseInput.performed += ctx =>
        {
            if (gameState == GameState.PAUSEMENU)
                SetPauseGame(false);
            else if (gameState == GameState.PLAYING)
                SetPauseGame(true);
        };
        pauseInput.Enable();

        OnGamePause += () =>
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
        OnGameUnpause += () =>
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
    public void RestartGame()
    {
        ClearAllActionSubscriptions();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void ClearAllActionSubscriptions()
    {
        OnGameStart = null;
        OnGamePause = null;
        OnGameUnpause = null;
        OnGameWin = null;
        OnGameRestart = null;
        OnGameExit = null;

        OnArrivedAtLocation = null;
        OnQuip = null;

        OnMove = null;
        OnMoveUp = null;
        OnMoveRight = null;
        OnMoveDown = null;
        OnMoveLeft = null;
        OnTryMoveWhileFreeSamples = null;
        OnTryExitBeforeShoppingListComplete = null;

        OnPickup = null;
        OnThrow = null;
        OnPocket = null;
        OnSmash = null;
        OnSpin = null;
        OnClap = null;
        OnThumbsUp = null;
        OnAngry = null;

        OnGotCorrectItem = null;
        OnGotWrongItem = null;
        OnShoppingListComplete = null;

        OnBlockedByFreeSamples = null;
        OnBlockedByMembershipEmployee = null;
        OnClearedBlockage = null;
        OnBlockedFromLeaving = null;
        OnAbleToLeave = null;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    #region Movement
    void MoveInputRecieved(Vector2 input)
    {
        if (gameState != GameState.PLAYING) return;

        if (input.y > 0.8f)
        {
            OnMove?.Invoke(currentLocation.upLocation != null && !curBlockedDirections[0]);

            MoveUp();
        }
        else if (input.x > 0.8f)
        {
            OnMove?.Invoke(currentLocation.rightLocation != null && !curBlockedDirections[1]);

            MoveRight();
        }
        else if (input.y < -0.8f)
        {
            OnMove?.Invoke(currentLocation.downLocation != null && !curBlockedDirections[2]);

            MoveDown();
        }
        else if (input.x < -0.8f)
        {
            OnMove?.Invoke(currentLocation.leftLocation != null && !curBlockedDirections[3]);

            MoveLeft();
        }
    }

    public void MoveUp()
    {
        TryDevourFreeSample();

        OnMoveUp?.Invoke();

        if (Time.time < afterFreeSampleKillMovementReenableTime) return;

        if (currentLocation == allLocations[allLocations.Length - 1])
        {
            if (IsShoppingListComplete())
            {
                gameState = GameState.WINMENU;

                OnGameWin?.Invoke();
            }
            else if (!curBlockedDirections[0])
            {
                OnTryExitBeforeShoppingListComplete?.Invoke();
            }
        }

        if (currentLocation.upLocation != null && !curBlockedDirections[0])
        {
            Location previousLocation = currentLocation;
            currentLocation = currentLocation.upLocation;
            ArriveAtLocation(previousLocation);
        }
    }
    public void MoveRight()
    {
        TryDevourFreeSample();

        OnMoveRight?.Invoke();

        if (Time.time < afterFreeSampleKillMovementReenableTime) return;

        if (currentLocation.rightLocation != null && !curBlockedDirections[1])
        {
            Location previousLocation = currentLocation;
            currentLocation = currentLocation.rightLocation;
            ArriveAtLocation(previousLocation);
        }
    }
    public void MoveDown()
    {
        TryDevourFreeSample();

        OnMoveDown?.Invoke();

        if (Time.time < afterFreeSampleKillMovementReenableTime) return;

        if (currentLocation.downLocation != null && !curBlockedDirections[2])
        {
            Location previousLocation = currentLocation;
            currentLocation = currentLocation.downLocation;
            ArriveAtLocation(previousLocation);
        }
    }
    public void MoveLeft()
    {
        TryDevourFreeSample();

        OnMoveLeft?.Invoke();

        if (Time.time < afterFreeSampleKillMovementReenableTime) return;

        if (currentLocation.leftLocation != null && !curBlockedDirections[3])
        {
            Location previousLocation = currentLocation;
            currentLocation = currentLocation.leftLocation;
            ArriveAtLocation(previousLocation);
        }
    }


    private void TryDevourFreeSample()
    {
        if (freeSampleLocations[currentLocation.index])
        {
            remainingButtonMashes--;
            Transform currentLocationFolder = itemLocationFolders[currentLocation.index].transform;
            FreeSamplesStand freeSamplesStand = currentLocationFolder.GetComponentInChildren<FreeSamplesStand>();
            freeSamplesStand.Attacked();

            OnTryMoveWhileFreeSamples?.Invoke();

            if (remainingButtonMashes <= 0)
            {
                ClearBlockedDirections();
                freeSamplesStand.Killed();

                afterFreeSampleKillMovementReenableTime = Time.time + afterFreeSampleKillMovementDisableTime;
            }
        }
    }
    #endregion

    #region Arriving at Location
    void ArriveAtLocation(Location previousLocation)
    {
        OnArrivedAtLocation?.Invoke(currentLocation);

        NewLocationQuip();

        DestroyObstaclesFromPreviousLocation(previousLocation);
        RefreshBlockedDirections(previousLocation);

        background.sprite = currentLocation.background;
        RefreshItemsAtCurrentLocation();

        InvokeSpecialEventsIfOnLastLocation();
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
        if (blockedLocations[currentLocation.index]) // Employee checking for membership blocks forward movement
        {
            curBlockedDirections[0] = true;

            OnBlockedByMembershipEmployee?.Invoke();
        }
        else if (freeSampleLocations[currentLocation.index]) // Free sample lady blocks all movement
        {
            remainingButtonMashes = buttonMashesToEscapeFreeSamples;

            for (int d = 0; d < curBlockedDirections.Length; d++)
                curBlockedDirections[d] = true;

            OnBlockedByFreeSamples?.Invoke();
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

        OnClearedBlockage?.Invoke(currentLocation);
    }

    void RefreshItemsAtCurrentLocation()
    {
        for (int l = 0; l < allLocations.Length; l++)
        {
            itemLocationFolders[l].SetActive(currentLocation.index == l);
        }
    }

    void InvokeSpecialEventsIfOnLastLocation()
    {
        if (currentLocation == allLocations[allLocations.Length - 1])
        {
            if (!IsShoppingListComplete())
            {
                OnBlockedFromLeaving?.Invoke();
            }
            else
            {
                OnAbleToLeave?.Invoke();
            }
        }
    }

    void NewLocationQuip()
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
                if (shoppingListCompletion[t])
                {
                    shoppingListTexts[t].color = Color.grey;
                    shoppingListTexts[t].fontStyle = FontStyles.Strikethrough;
                }
                else
                {
                    shoppingListTexts[t].color = Color.white;
                    shoppingListTexts[t].fontStyle = FontStyles.Normal;
                }
            }
            else
                shoppingListTexts[t].text = "";
        }
    }

    bool IsShoppingListComplete()
    {
        bool allItemsGot = true;
        foreach (bool s in shoppingListCompletion)
        {
            if (!s)
            {
                allItemsGot = false;
                break;
            }
        }

        return allItemsGot;
    }
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
        if (gameState != GameState.PLAYING) return;

        if (heldItem == null)
            Pickup();
        else
            Throw();
    }
    void Pickup()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        //Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.red, 100);

        if (Physics.Raycast(ray, out RaycastHit hit, 500, itemLayer) && hit.transform.CompareTag("CostcoProduct"))
        {
            heldItem = hit.transform.gameObject;
            heldItem.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            heldItem.GetComponent<Rigidbody>().velocity = Vector3.zero;
            hit.transform.parent = itemHoldLoc;

            // Disabling sparkle graphic behind all untouched items
            if (heldItem.GetComponentInChildren<SpriteRenderer>() != null)
                heldItem.GetComponentInChildren<SpriteRenderer>().gameObject.SetActive(false);

            OnPickup?.Invoke(heldItem.name);
        }
    }
    void Throw()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        //Debug.DrawLine(Camera.main.transform.position, Mouse.current.position.ReadValue(), Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, 500, backgroundLayer) && hit.collider.CompareTag("ThrowableSurface"))
        {
            heldItem.transform.parent = itemLocationFolders[currentLocation.index].transform;

            float spinForceMultiplier = Mathf.Clamp(curSpinSpeed / baseSpinSpeed, 1, 2);
            heldItem.GetComponent<Rigidbody>().AddForce(ray.direction * throwForce * spinForceMultiplier, ForceMode.Impulse);
            heldItem.GetComponent<Rigidbody>().AddTorque(Vector3.right * -150 * spinForceMultiplier, ForceMode.Impulse);

            heldItem = null;

            OnThrow?.Invoke();
        }

    }
    public void Pocket()
    {
        if (gameState != GameState.PLAYING) return;
        if (heldItem == null) return;

        bool itemFoundInShoppingList = false;
        string itemName = heldItem.name;
        for (int s = 0; s < shoppingListItems.Length && !itemFoundInShoppingList; s++)
        {
            if (!shoppingListCompletion[s] && itemName == shoppingListItems[s].itemName)
            {
                shoppingListCompletion[s] = true;
                RefreshShoppingListTexts();

                if (IsShoppingListComplete())
                {
                    OnShoppingListComplete?.Invoke();
                }

                InvokeSpecialEventsIfOnLastLocation();

                OnGotCorrectItem?.Invoke();

                itemFoundInShoppingList = true;
            }
        }
        if (!itemFoundInShoppingList)
        {
            numIncorrectItemsAddedToCart++;

            OnGotWrongItem?.Invoke();
        }

        OnPocket?.Invoke();

        heldItem.transform.parent = shoppingCartStorage;
        heldItem.SetActive(false);
        heldItem = null;
    }
    public void Smash()
    {
        if (gameState != GameState.PLAYING) return;
        if (heldItem == null) return;

        bool heldItemNeededForShoppingList = false;
        for (int s = 0; s < shoppingListItems.Length; s++)
        {
            if (shoppingListItems[s].itemName == heldItem.name && !shoppingListCompletion[s])
            {
                heldItemNeededForShoppingList = true;
                break;
            }
        }
        if (heldItemNeededForShoppingList) return;

        OnSmash?.Invoke();

        heldItem.transform.parent = shoppingCartStorage;
        heldItem.SetActive(false);
        heldItem = null;
    }
    public void Spin()
    {
        if (gameState != GameState.PLAYING) return;
        if (heldItem == null) return;

        curSpinSpeed = maxSpinSpeed;

        OnSpin?.Invoke();
    }
    #endregion

    #region Emotes
    public void Clap()
    {
        if (heldItem != null) return;
        if (gameState != GameState.PLAYING) return;

        OnClap?.Invoke();
    }
    public void ThumbsUp()
    {
        if (heldItem != null) return;
        if (gameState != GameState.PLAYING) return;

        OnThumbsUp?.Invoke();
    }
    public void Angry()
    {
        if (heldItem != null) return;
        if (gameState != GameState.PLAYING) return;

        OnAngry?.Invoke();
    }
    #endregion

    public int GetNumberOfIncorrectItemsAddedToCart() { return numIncorrectItemsAddedToCart; }
}
