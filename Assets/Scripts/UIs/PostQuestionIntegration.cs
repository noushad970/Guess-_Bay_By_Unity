using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class QuestionCorrectAnswerToggle : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Toggle[] toggles;
    [SerializeField] private TMP_InputField[] inputs;
    private List<string> answers = new List<string>();
    private int toggleIndex;
    [SerializeField] private TMP_InputField QuestionText;
    [SerializeField] private TMP_InputField rewardSelectionText;
    [SerializeField] private QuestionManager questionManager;
    [SerializeField] private UnityEngine.UI.Button submitAPostButton;
    void Start()
    {
        toggleIndex = -1;
        for (int i = 0; i < toggles.Length; i++)
        {
            UnityEngine.UI.Toggle toggle = toggles[i];
            toggle.onValueChanged.AddListener((isOn) => OnToggleChanged(toggle));

        }
        submitAPostButton.onClick.AddListener(postAQuestion);

    }
    private void postAQuestion()
    {
        if (checkCanPostQuestion() && toggleIndex!=-1)
        {
            Debug.Log("Data given for posting!");
            questionManager.PostQuestion(QuestionText.text, answers, int.Parse(rewardSelectionText.text), inputs[toggleIndex].text.ToString());
            toggleIndex = -1;
        }
        else
        {
            Debug.Log("Add more questions answer or Select correct answer!");
        }
    }
    private void Update()
    {

    }
    void OnToggleChanged(UnityEngine.UI.Toggle changedToggle)
    {
        if (changedToggle.isOn)
        {
           
            for (int i = 0; i < 16; i++)
            {
                UnityEngine.UI.Toggle toggle = toggles[i];
                if (toggle != changedToggle)
                {
                    toggle.isOn = false;

                }
                else
                {
                    toggleIndex= i;
                }
            }
        }
    }
    public int GetToggleIndex()
    {
        return toggleIndex;
    }
    public string getQuestionCorrectAnswer()
    {
        return inputs[toggleIndex].text.ToString();
    }
    public List<string> getQuestionAllAnswer()
    {
        return answers;
    }
    public bool checkCanPostQuestion()
    {
        int totAnsgiven = 0;
        for(int i=0; i<inputs.Length; i++)
        {
            if (inputs[i] != null)
            {
                totAnsgiven++;
                answers.Add(inputs[i].text.ToString());
            }
        }
        if (totAnsgiven >= 4)
        {
            return true;
        }
        else { return false; }
    }
    public string getQuestionText()
    {
        return QuestionText.text.ToString();
    }
    public int getRewardForQuestion()
    {
        return int.Parse(rewardSelectionText.text);
    }
}
