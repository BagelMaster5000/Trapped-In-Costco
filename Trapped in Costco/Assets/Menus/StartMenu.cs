using HauntedPSX.RenderPipelines.PSX.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class StartMenu : Menu
{
    [Header("Settings")]
    [SerializeField] Toggle showControlsToggle;

    [SerializeField] Toggle crtToggle;
    [SerializeField] VolumeProfile globalVolumeProfile;

    public override void Start()
    {
        base.Start();

        showControlsToggle.SetIsOnWithoutNotify(GlobalVariables.showControls);
        ApplyShowHideControls();

        crtToggle.SetIsOnWithoutNotify(GlobalVariables.crtFilter);
        ApplyCRTFilter();
    }

    public void StartGame()
    {
        GameController.staticReference.StartGame();

        Disappear();
    }

    public void ExitGame() => GameController.staticReference.ExitGame();

    public void ToggleControls()
    {
        GlobalVariables.showControls = !GlobalVariables.showControls;
        showControlsToggle.SetIsOnWithoutNotify(GlobalVariables.showControls);
        ApplyShowHideControls();
    }
    void ApplyShowHideControls()
    {
        GameObject[] allControlTexts = GameObject.FindGameObjectsWithTag("ControlText");
        for (int c = 0; c < allControlTexts.Length; c++)
        {
            if (allControlTexts[c].GetComponent<Image>())
                allControlTexts[c].GetComponent<Image>().enabled = GlobalVariables.showControls;
            if (allControlTexts[c].GetComponentInChildren<TextMeshProUGUI>())
                allControlTexts[c].GetComponentInChildren<TextMeshProUGUI>().enabled = GlobalVariables.showControls;
        }
    }

    public void ToggleCRTFilter()
    {
        GlobalVariables.crtFilter = !GlobalVariables.crtFilter;
        crtToggle.SetIsOnWithoutNotify(GlobalVariables.crtFilter);
        ApplyCRTFilter();
    }
    void ApplyCRTFilter()
    {
        globalVolumeProfile.TryGet(out QualityOverrideVolume v);
        v.active = GlobalVariables.crtFilter;
    }
}
