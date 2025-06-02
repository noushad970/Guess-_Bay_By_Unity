using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using TMPro;

public class ForYouPage : MonoBehaviour
{
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button playButton,nextButton;


    [Header("Prize and Entry Fee")]
    [SerializeField] private TMP_Text entryFeeText, prizeMoneyText, WinPrizeText, NoAnwerPrizeText;
    [SerializeField] private QuestionManager questionManager;
    void Start()
    {
        playButton.onClick.AddListener(onClickPlayButton);
        nextButton.onClick.AddListener(onClickNextButton);
    }
    public void onClickNextButton()
    {
        questionManager.LoadForYou();
    }
    public void onClickPlayButton()
    {
        //goto play section
    }
    public void initializeQuestions()
    {
        int questionPrize= questionManager.getQuestionPrize();
        prizeMoneyText.text= "Prize Money: "+questionPrize.ToString()+" Tokens";
        entryFeeText.text = "Entry Fee: "+(questionPrize + 3).ToString()+" Tokens";
        WinPrizeText.text = "Win Prize: "+(questionPrize + questionPrize).ToString()+" Tokens";
        NoAnwerPrizeText.text= "No Answer: "+(questionPrize/2).ToString()+" Tokens";
    }
    public void initializeFeeAndPrize()
    {
        string question=questionManager.getQuestionText();
        questionText.text= question;
    }
    public void initializeNoAnswer()
    {
        prizeMoneyText.text ="Prize Money:";
        entryFeeText.text = "Entry Fee:";
        WinPrizeText.text = "Win Prize:";
        NoAnwerPrizeText.text = "No Answer:";
        questionText.text = "No Question available for you right now. Try later to get questions. Thank you!";
    }
}
