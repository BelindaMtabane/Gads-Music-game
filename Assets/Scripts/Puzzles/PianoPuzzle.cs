using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PianoPuzzle : MonoBehaviour
{
    public TMP_Text patternText;
    public TMP_Text inputText;
    public TMP_Text resultText;
    public TMP_Text timerText;

    public GameObject[] doors;
    private int doorIndex = 0;

    private float timeLimit = 5f;
    private float timer = 0f;

    private bool roundActive = false;
    private int count = 3;
    private float roundDelayTimer = 0f;
    private bool waitingForRound = true;

    private KeyCode[] keys =
    {
        KeyCode.T,
        KeyCode.Y,
        KeyCode.U,
        KeyCode.I,
        KeyCode.O
    };

    private List<KeyCode> pattern = new List<KeyCode>();
    private List<KeyCode> input = new List<KeyCode>();
    private int index = 0;

    void Start()
    {
        // Start initial 20 second delay
        waitingForRound = true;
        roundDelayTimer = 0f;
    }

    void Update()
    {
        HandleRoundDelay();
        HandleTimer();

        if (!roundActive) return;

        CheckInput(KeyCode.T);
        CheckInput(KeyCode.Y);
        CheckInput(KeyCode.U);
        CheckInput(KeyCode.I);
        CheckInput(KeyCode.O);
    }

    // ---------------- START ROUND ----------------
    public void StartRound()
    {
        pattern.Clear();
        input.Clear();

        index = 0;
        timer = 0f;

        roundActive = true;
        waitingForRound = false;

        resultText.text = "";
        inputText.text = "";

        for (int i = 0; i < count; i++) // or change difficulty later
        {
            pattern.Add(keys[Random.Range(0, keys.Length)]);
        }

        ShowPattern();
    }

    void ShowPattern()
    {
        patternText.text = "";

        foreach (var k in pattern)
        {
            patternText.text += k + " ";
        }
    }

    // ---------------- INPUT ----------------
    void CheckInput(KeyCode key)
    {
        if (!Input.GetKeyDown(key)) return;

        input.Add(key);
        inputText.text += key + " ";

        if (key != pattern[index])
        {
            FailRound("ALARM! WRONG KEY");
            return;
        }

        index++;

        if (index >= pattern.Count)
        {
            WinRound();
        }
    }

    // ---------------- TIMER ----------------
    void HandleTimer()
    {
        if (!roundActive) return;

        timer += Time.deltaTime;

        timerText.text = "Time: " + (timeLimit - timer).ToString("F2");

        if (timer >= timeLimit)
        {
            FailRound("TIME UP!");
        }
    }

    // ---------------- WIN ----------------
    void WinRound()
    {
        resultText.text = "DOOR OPENED!";
        roundActive = false;

        OpenNextDoor();
        count++;

        waitingForRound = true;
        roundDelayTimer = 0f;
    }

    // ---------------- FAIL ----------------
    void FailRound(string message)
    {
        resultText.text = message;
        roundActive = false;

        // fail = shorter delay OR instant retry delay
        Invoke(nameof(StartFailDelay), 2f);
    }

    void StartFailDelay()
    {
        waitingForRound = true;
        roundDelayTimer = 0f;
    }

    // ---------------- DOORS ----------------
    void OpenNextDoor()
    {
        if (doorIndex >= doors.Length) return;

        doors[doorIndex].SetActive(false);
        doorIndex++;
    }

    // ---------------- DELAY SYSTEM ----------------
    void HandleRoundDelay()
    {
        if (!waitingForRound) return;

        roundDelayTimer += Time.deltaTime;

        timerText.text = "Starting in: " + Mathf.Max(0, 10f - roundDelayTimer).ToString("F1");

        if (roundDelayTimer >= 10f)
        {
            waitingForRound = false;
            roundDelayTimer = 0f;
            StartRound();
        }
    }
}
