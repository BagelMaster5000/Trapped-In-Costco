using UnityEngine;
using UnityEngine.UI;

public class MoveArrowVisibilityController : MonoBehaviour
{
    [SerializeField] GameObject[] arrows = new GameObject[4]; // up, right, down, left
    Image[] arrowImages = new Image[4];
    Color baseColor;

    private void Awake()
    {
        for (int a = 0; a < 4; a++) { arrowImages[a] = arrows[a].GetComponent<Image>(); }
        baseColor = arrowImages[0].color;

        GameController.staticReference.OnArrivedAtLocation += ArrivedAtLocation;

        GameController.staticReference.OnBlockedByFreeSamples += RedAllArrows;
        GameController.staticReference.OnClearedFreeSamples += ArrivedAtLocation;
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
        for (int a = 0; a < arrows.Length; a++)
        {
            arrows[a].SetActive(directionsToShow[a]);
            arrowImages[a].color = baseColor;
        }
    }
    public void ShowArrows()
    {
        for (int a = 0; a < arrows.Length; a++)
        {
            arrows[a].SetActive(true);
            arrowImages[a].color = baseColor;
        }
    }

    public void RedAllArrows()
    {
        for (int a = 0; a < arrows.Length; a++)
        {
            arrows[a].SetActive(true);
            arrowImages[a].color = Color.red;
        }
    }
}
