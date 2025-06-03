using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayGamePage : MonoBehaviour
{
    [SerializeField] private QuestionManager questionManager;
    [SerializeField] private GameObject[] answerObjects;
    [SerializeField] private TMP_Text[] answerTexts;
    [SerializeField] private Button submitButton, ReduceQuestionButton, ExtendTimerButton;
    [SerializeField] private List<Toggle> toggles;
    [SerializeField] private Image fillImage; // The UI Image with Fill type set
    [SerializeField] private TMP_Text timerCount;
    [SerializeField] private GameObject gameSection;
    [SerializeField] private PanelManager panelManager;
    private float duration; // Duration in seconds (n)
    private string userSelectedAnswer;
    [Header("Pages")]

    [SerializeField] private GameObject homeSection;

    private void Start()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            Toggle toggle = toggles[i];
            toggle.onValueChanged.AddListener((isOn) => OnToggleChanged(toggle, isOn));
        }

        StartTimer();
        StartFill();
        setVisibleAnswerAndHideNullAnswer();
        ReduceQuestionButton.onClick.AddListener(onClickReduceQuestion);
        ExtendTimerButton.onClick.AddListener(onClickExntendTimer);
        submitButton.onClick.AddListener(onClickSubmitButton);
    }
    
    private void Update()
    {
        timerCount.text=((int)remainingTime).ToString();
        Debug.Log("Toggle Answer Value: " + userSelectedAnswer);
        if (remainingTime <= 0)
        {
            //no answer is given and game over;
            Debug.Log("No answer is Given");
            panelManager.disableAll();
            gameSection.SetActive(false); 
            homeSection.SetActive(true);
            //no answer UI
        }
    }

    public void startGame()
    {

        panelManager.disableAll();
        gameSection.SetActive(true);
        StartTimer();
        StartFill();
        setVisibleAnswerAndHideNullAnswer();
    }
    private void setVisibleAnswerAndHideNullAnswer()
    {
    //    List<string> allAnswer= new List<string>();
    //    allAnswer = questionManager.getAllAnswers();
        for (int i = 0; i < 16; i++)
        {
            answerObjects[i].SetActive(false);
        }
        for(int i=0;i<questionManager.getAnswersCount();i++)
        {
            answerObjects[i].SetActive(true);
            answerTexts[i].text = questionManager.getAllAnswers()[i];
        }
    }
    private int clickCounter=0;
    private void onClickReduceQuestion()
    {
        int rand=Random.Range(0,questionManager.getAnswersCount());
        if (answerTexts[rand].text == questionManager.getCorrectAnswer() || answerObjects[rand].activeSelf==false)
        {
            
            clickCounter++;
            if (clickCounter >= 16)
            {
                return;
            }
            else
            {
                onClickReduceQuestion();
            }
        }
        else
        {
            clickCounter = 0;
            answerObjects[rand].SetActive(false);
        }
    }
    private void onClickExntendTimer()
    {
        ExtendTimerButton.gameObject.SetActive(false);
        AddTime(30);
        duration = remainingTime;
    }
    [SerializeField] private KeyValuePair<Toggle, TMP_Text>[] answerPairWithToggle; 
    private void onClickSubmitButton()
    {
        if (userSelectedAnswer == questionManager.getCorrectAnswer())
        {
            Debug.Log("Congratulation you have won. the prize");
            gameSection.SetActive(false);
            //won UI

            homeSection.SetActive(true);
        }
        else
        {
            Debug.Log("Sorry wrong answer! Try Again my friend");
            gameSection.SetActive(false);
            //Loss UI
            homeSection.SetActive(true);
        }
    }
    void OnToggleChanged(Toggle changedToggle, bool isOn)
    {
        if (isOn)
        {
            userSelectedAnswer = changedToggle.gameObject.GetComponentInChildren<TMP_Text>().text;
            foreach (Toggle toggle in toggles)
            {
                if (toggle != changedToggle)
                {
                    toggle.isOn = false;
                }
            }
        }
    }

    
    public void StartFill()
    {
        duration = remainingTime;
        StartCoroutine(FillOverTime());
    }

    private IEnumerator FillOverTime()
    {
        float timer = 0f;
        fillImage.fillAmount = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            fillImage.fillAmount = Mathf.Clamp01(timer / duration);
            yield return null;
        }

        fillImage.fillAmount = 1f; // Ensure it's exactly full at the end
    }


    //timer functionalities

    private float remainingTime = 0f;
    private bool isTimerRunning = false;
    private Coroutine timerCoroutine;

    // Call this to start the timer
    public void StartTimer(float duration = 60f)
    {
        if (isTimerRunning)
        {
            Debug.Log("Timer is already running.");
            return;
        }

        remainingTime = duration;
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    // Call this to add time to the running timer
    public void AddTime(float additionalTime)
    {
        if (isTimerRunning)
        {
            remainingTime += additionalTime;
            Debug.Log("Time added. New remaining time: " + remainingTime);
        }
        else
        {
            Debug.Log("Timer is not running. Cannot add time.");
        }
    }

    private IEnumerator TimerRoutine()
    {
        isTimerRunning = true;
        Debug.Log("Timer started for " + remainingTime + " seconds.");

        while (remainingTime > 0f)
        {
            yield return null;
            remainingTime -= Time.deltaTime;
        }

        isTimerRunning = false;
        Debug.Log("Timer finished.");
    }

    // Optional: Get current remaining time
    public float GetRemainingTime()
    {
        return remainingTime;
    }
}
