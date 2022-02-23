using System.Collections;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    float curTime = 0;

    [SerializeField] TextMeshProUGUI timerText;
    float timerTextRefreshInterval = 0.05f;

    void Start()
    {
        StartCoroutine(RefreshTimerTextLoop());
    }

    void Update()
    {
        if (GameController.staticReference.gameState == GameController.GameState.PLAYING)
        {
            curTime += Time.deltaTime;
        }
    }

    IEnumerator RefreshTimerTextLoop()
    {
        while (true)
        {
            int curMinutes = Mathf.FloorToInt(curTime) / 60;
            int curSeconds = Mathf.FloorToInt(curTime) % 60;
            int curMilliseconds = Mathf.FloorToInt(curTime * 100) % 100;
            timerText.text = curMinutes + ":" +
                curSeconds.ToString("00") + "." +
                "<size=" + (timerText.fontSize - 10) + ">" + curMilliseconds.ToString("00") + "</size>";

            yield return new WaitForSeconds(timerTextRefreshInterval);
        }
    }

    public float GetTimeSeconds() { return curTime; }
    public string GetTimeFormatted()
    {
        int curMinutes = Mathf.FloorToInt(curTime) / 60;
        int curSeconds = Mathf.FloorToInt(curTime) % 60;
        int curMilliseconds = Mathf.FloorToInt(curTime * 100) % 100;
        string timeFormatted = curMinutes + ":" + curSeconds.ToString("00") + "." + curMilliseconds.ToString("00");

        return timeFormatted;
    }
}
