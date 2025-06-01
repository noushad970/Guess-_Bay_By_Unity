using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class WheelManager : MonoBehaviour {

    //Creates the wheel
    SpinWheel wheel = new SpinWheel(8);
    int money = 300;
    [SerializeField] private Text text;
    [SerializeField] private GameObject go;
    [SerializeField] private GameObject win;
    [SerializeField] private TMP_Text winT;
    [SerializeField] private GameObject spinButton;
    [SerializeField] private AnonymousSignIn user;
	void Start () {
        ////Keep track of the player money
        //PlayerPrefs.SetInt("money", 500000);
        //money = PlayerPrefs.GetInt("money", 500000);
        
        UpdateText();

        //Sets the gameobject
        wheel.setWheel(gameObject);

        //Sets the callback
        wheel.AddCallback((index) => {
            switch (index)
            {
                case 1:
                    win.SetActive(true);
                    winT.text = "watch Ads";
                    //playing ads
                    break;
                case 2:
                    user.AddTokens(25);
                    win.SetActive(true);
                    winT.text = "Jackpot! You Got 25 Tokens.";
                    break;
                case 3:
                    user.AddTokens(1);
                    win.SetActive(true);
                    winT.text = "Congratulations! You Got 1 Token.";
                    break;
                case 4:
                    user.AddSpin(1);
                    win.SetActive(true);
                    winT.text = "Congratulations! You Got a Free Spin.";
                    break;
                case 5:
                    user.AddTokens(5);
                    win.SetActive(true);
                    winT.text = "Congratulations! You Got 5 Tokens.";
                    break;
                case 6:
                    user.AddSpin(1);
                    win.SetActive(true);
                    winT.text = "Congratulations! You Got a Free Spin.";
                    break;
                case 7:
                    user.AddTokens(2);
                    win.SetActive(true);
                    winT.text = "Congratulations! You Got 2 Tokens.";
                    break;
                case 8:
                    user.AddTokens(10);
                    win.SetActive(true);
                    winT.text = "Congratulations! You Got 10 Tokens.";
                    break;
            }
            UpdateText();
        });
	}
    private void Update()
    {

        spinButton.SetActive(!SpinWheel.isSpinning);
        money = user.getTotalSpin();
        UpdateText();
    }
    public void UpdateText()
    {
        text.text = money + "";
    }

    public void OkWin()
    {
        win.SetActive(false);
    }

    public void Spin()
    {
        if (money >= 1)
        {
            money -= 1;
            user.RemoveSpin(1);
            StartCoroutine(wheel.StartNewRun());
            UpdateText();
        } else
        {
            go.SetActive(true);
        }
    }

    public void ok()
    {
        go.SetActive(false);
    }
}
