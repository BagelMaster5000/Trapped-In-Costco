using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoveArrowVisibilityController : MonoBehaviour
{
    [SerializeField] GameObject[] arrows = new GameObject[4]; // up, right, down, left
    Image[] arrowImages = new Image[4];
    Color baseColor;

    Coroutine flashingArrows;
    const float arrowFlashInterval = 0.1f;

    private void Awake()
    {
        for (int a = 0; a < 4; a++) { arrowImages[a] = arrows[a].GetComponent<Image>(); }
        baseColor = arrowImages[0].color;

        GameController.staticReference.OnArrivedAtLocation += ArrivedAtLocation;

        GameController.staticReference.OnBlockedByFreeSamples += RedAllArrowsAndFlash;
        GameController.staticReference.OnBlockedByMembershipEmployee += RedForwardArrow;
        GameController.staticReference.OnClearedBlockage += ArrivedAtLocation;

        GameController.staticReference.OnBlockedFromLeaving += RedForwardArrow;
        GameController.staticReference.OnAbleToLeave += ForceShowForwardArrow;
    }

    void Start()
    {
        if (arrows.Length != 4)
            Debug.LogError("There should be 4 arrow objects attached to MoveArrowVisibilityController!");
    }

    void ArrivedAtLocation(Location curLocation)
    {
        bool[] validLocations = new bool[4];
        if (curLocation.upLocation != null) validLocations[0] = true;
        if (curLocation.rightLocation != null) validLocations[1] = true;
        if (curLocation.downLocation != null) validLocations[2] = true;
        if (curLocation.leftLocation != null) validLocations[3] = true;

        ShowArrows(validLocations);
    }

    // bool array is: up, right, down, left
    public void ShowArrows(bool[] directionsToShow)
    {
        if (flashingArrows != null) StopCoroutine(flashingArrows);

        for (int a = 0; a < arrows.Length; a++)
        {
            arrows[a].SetActive(directionsToShow[a]);
            arrowImages[a].color = baseColor;
        }
    }
    public void ShowArrows()
    {
        if (flashingArrows != null) StopCoroutine(flashingArrows);

        for (int a = 0; a < arrows.Length; a++)
        {
            arrows[a].SetActive(true);
            arrowImages[a].color = baseColor;
        }
    }
    public void ForceShowForwardArrow()
    {
        if (flashingArrows != null) StopCoroutine(flashingArrows);

        arrows[0].SetActive(true);
        arrowImages[0].color = baseColor;
    }

    public void RedAllArrowsAndFlash()
    {
        for (int a = 0; a < arrows.Length; a++)
        {
            arrows[a].SetActive(true);
        }

        if (flashingArrows != null) StopCoroutine(flashingArrows);
        flashingArrows = StartCoroutine(FlashingAllArrows());
    }
    IEnumerator FlashingAllArrows()
    {
        while (true)
        {
            for (int a = 0; a < arrows.Length; a++) { arrowImages[a].color = Color.red; }
            yield return new WaitForSeconds(arrowFlashInterval);
            for (int a = 0; a < arrows.Length; a++) { arrowImages[a].color = Color.white; }
            yield return new WaitForSeconds(arrowFlashInterval);
        }
    }
    public void RedForwardArrow()
    {
        if (flashingArrows != null) StopCoroutine(flashingArrows);

        arrows[0].SetActive(true);
        arrowImages[0].color = Color.red;
    }
}
