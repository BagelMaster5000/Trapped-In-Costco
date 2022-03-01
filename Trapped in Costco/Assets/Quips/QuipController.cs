using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuipController : MonoBehaviour
{
    [SerializeField] Image backingGraphic;
    float backingGraphicStartAlpha;
    [SerializeField] TextMeshProUGUI quipText;

    [Header("Animation")]
    [SerializeField] float quipStartDelay = 0;
    [SerializeField] float quipEndDelay = 1;

    [Space(5)]
    [SerializeField] float quipFadeInTime = 1;
    [SerializeField] float quipFadeOutTime = 1;

    [Space(5)]
    [SerializeField] float quipLetterDisplayInterval = 0.05f;

    [Header("Special Quips")]
    [SerializeField] string[] tryingToLeaveCostcoEarlyQuips;
    [SerializeField] string[] blockedByFreeSampleStandQuips;
    [SerializeField] string[] blockedByMembershipEmployeeQuips;

    bool playingAnimation;

    Queue<string> quipQueue = new Queue<string>();

    public System.Action OnQuipSound;

    private void Awake()
    {
        GameController.staticReference.OnQuip += AddQuip;
        GameController.staticReference.OnArrivedAtLocation += ClearAllQuips;

        GameController.staticReference.OnTryExitBeforeShoppingListComplete += () =>
        {
            if (tryingToLeaveCostcoEarlyQuips.Length > 0)
            {
                AddQuip(tryingToLeaveCostcoEarlyQuips[Random.Range(0, tryingToLeaveCostcoEarlyQuips.Length)]);

                TryStartAnimationForNextQuip();
            }
        };
        GameController.staticReference.OnBlockedByFreeSamples += () =>
        {
            if (blockedByFreeSampleStandQuips.Length > 0)
            {
                AddQuip(blockedByFreeSampleStandQuips[Random.Range(0, blockedByFreeSampleStandQuips.Length)]);

                TryStartAnimationForNextQuip();
            }
        };
        GameController.staticReference.OnBlockedByMembershipEmployee += () =>
        {
            if (blockedByMembershipEmployeeQuips.Length > 0)
            {
                AddQuip(blockedByMembershipEmployeeQuips[Random.Range(0, blockedByMembershipEmployeeQuips.Length)]);

                TryStartAnimationForNextQuip();
            }
        };
    }

    private void Start()
    {
        backingGraphicStartAlpha = backingGraphic.color.a;
        backingGraphic.CrossFadeAlpha(0, 0, true);
        quipText.CrossFadeAlpha(0, 0, true);
    }


    void AddQuip(string quip)
    {
        quipQueue.Enqueue(quip);

        TryStartAnimationForNextQuip();
    }

    void ClearAllQuips(Location dummy)
    {
        quipQueue.Clear();
    }

    void TryStartAnimationForNextQuip()
    {
        if (quipQueue.Count > 0 && !playingAnimation)
        {
            while (quipQueue.Count > 1) { quipQueue.Dequeue(); } // Clear old quips to prevent buildup

            StopAllCoroutines();
            StartCoroutine(DisplayQuipAnimation(quipQueue.Dequeue()));
        }
    }
    IEnumerator DisplayQuipAnimation(string quip)
    {
        quipText.text = "";
        backingGraphic.CrossFadeAlpha(0, 0, true);
        quipText.CrossFadeAlpha(0, 0, true);

        backingGraphic.CrossFadeAlpha(backingGraphicStartAlpha, quipFadeInTime, false);
        quipText.CrossFadeAlpha(1, quipFadeInTime, false);

        yield return new WaitForSeconds(quipStartDelay);
        playingAnimation = true;

        int quipSoundInterval = 10, quipSoundCountdown = 0;
        int quipCharactersDisplayed = 1;
        while (quipCharactersDisplayed <= quip.Length)
        {
            quipText.text = quip.Substring(0, quipCharactersDisplayed) +
                "<alpha=100>" +
                quip.Substring(quipCharactersDisplayed, quip.Length - quipCharactersDisplayed);

            quipSoundCountdown--;
            if (quipSoundCountdown <= 0)
            {
                quipSoundCountdown = quipSoundInterval;
                OnQuipSound?.Invoke();
            }

            yield return new WaitForSeconds(quipLetterDisplayInterval);
            quipCharactersDisplayed++;
        }

        yield return new WaitForSeconds(quipEndDelay);

        backingGraphic.CrossFadeAlpha(0, quipFadeOutTime, false);
        quipText.CrossFadeAlpha(0, quipFadeOutTime, false);
        yield return new WaitForSeconds(quipFadeOutTime);
        playingAnimation = false;


        TryStartAnimationForNextQuip();
    }
}
