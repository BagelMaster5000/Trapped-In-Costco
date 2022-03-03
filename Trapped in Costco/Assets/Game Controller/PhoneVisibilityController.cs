using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneVisibilityController : MonoBehaviour
{
    [SerializeField] Animator phoneAnimator;
    [SerializeField] InputAction phoneToggleInput;

    public Action OnPhoneVisible;
    public Action OnPhoneInvisible;

    [SerializeField] TextMeshProUGUI phoneAboveText;

    private void Awake()
    {
        GameController.staticReference.OnGamePause += () =>
        {
            phoneToggleInput.Disable();
        };
        GameController.staticReference.OnGameUnpause += () =>
        {
            phoneToggleInput.Enable();
        };

        GameController.staticReference.OnGameRestart += () =>
        {
            OnPhoneVisible = null;
            OnPhoneInvisible = null;
        };

        GameController.staticReference.OnShoppingListComplete += () =>
        {
            phoneAboveText.text = "Time to leave!";
            phoneAboveText.color = Color.green;
        };
    }

    private void Start()
    {
        phoneToggleInput.performed += ctx => ToggleVisibility();
        phoneToggleInput.Enable();
    }

    public void ToggleVisibility()
    {
        if (GetIsVisible()) HidePhoneScreen();
        else ShowPhoneScreen();
    }
    public void ShowPhoneScreen()
    { 
        phoneAnimator.SetBool("Up", true);

        OnPhoneVisible?.Invoke();
    }
    public void HidePhoneScreen()
    {
        phoneAnimator.SetBool("Up", false);

        OnPhoneInvisible?.Invoke();
    }

    public bool GetIsVisible() { return phoneAnimator.GetBool("Up"); }
}
