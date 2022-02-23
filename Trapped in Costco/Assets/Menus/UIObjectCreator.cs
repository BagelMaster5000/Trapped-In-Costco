using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIObjectCreator : MonoBehaviour
{
    [SerializeField] GameObject StartMenu;
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject WinMenu;

    private void Start()
    {
        if (StartMenu != null)
            Instantiate(StartMenu, transform);

        if (PauseMenu != null)
            Instantiate(PauseMenu, transform);

        if (WinMenu != null)
            Instantiate(WinMenu, transform);
    }
}
