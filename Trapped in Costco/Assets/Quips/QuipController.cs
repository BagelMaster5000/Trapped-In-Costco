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

    bool playingAnimation;

    Queue<string> quipQueue = new Queue<string>();

    private void Awake()
    {
        GameController.staticReference.OnQuip += AddQuip;
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

    void TryStartAnimationForNextQuip()
    {
        if (quipQueue.Count > 0 && !playingAnimation)
        {
            StartCoroutine(DisplayQuipAnimation(quipQueue.Dequeue()));
        }
    }
    IEnumerator DisplayQuipAnimation(string quip)
    {
        quipText.text = "";
        backingGraphic.CrossFadeAlpha(0, 0, true);
        quipText.CrossFadeAlpha(0, 0, true);


        playingAnimation = true;
        backingGraphic.CrossFadeAlpha(backingGraphicStartAlpha, quipFadeInTime, false);
        quipText.CrossFadeAlpha(1, quipFadeInTime, false);

        yield return new WaitForSeconds(quipStartDelay);

        int quipCharactersDisplayed = 1;
        while (quipCharactersDisplayed <= quip.Length)
        {
            quipText.text = quip.Substring(0, quipCharactersDisplayed) +
                "<alpha=100>" +
                quip.Substring(quipCharactersDisplayed, quip.Length - quipCharactersDisplayed);

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
