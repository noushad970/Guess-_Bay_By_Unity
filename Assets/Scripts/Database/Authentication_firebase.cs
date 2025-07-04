using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;

public class Authentication_firebase : MonoBehaviour
{
    public TMP_InputField phoneInp, EnterCode_Inp;
    public TextMeshProUGUI logTxt;
    public GameObject AllOptions;//all options
    public GameObject SuccessUi;
    uint phoneAuthTimeoutMs = 3 * 60000;//minutes to milisec
    FirebaseAuth auth;
    PhoneAuthProvider provider;
    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        provider = PhoneAuthProvider.GetInstance(auth);
    }

    public void sendSMS()
    {
        print("send sms pressed");

        string userNumber = "+88" + phoneInp.text;

        PhoneAuthProvider provider = PhoneAuthProvider.GetInstance(auth);
        provider.VerifyPhoneNumber(
          new PhoneAuthOptions
          {
              PhoneNumber = userNumber,
              TimeoutInMilliseconds = 60,
              ForceResendingToken = null
          },
          verificationCompleted: (credential) => {
              print("Verification completed");
              showLogMsg("Verification completed");

              // Auto-sms-retrieval or instant validation has succeeded (Android only).
              // There is no need to input the verification code.
              // `credential` can be used instead of calling GetCredential().
          },
          verificationFailed: (error) => {
              print("Error: " + error);
              showLogMsg(error);

              // The verification code was not sent.
              // `error` contains a human readable explanation of the problem.
          },
          codeSent: (id, token) => {
              showLogMsg("Code send success!!");
              // Verification code was successfully sent via SMS.
              // `id` contains the verification id that will need to passed in with
              PlayerPrefs.SetString("verificationId", id);
              // the code from the user when calling GetCredential().
              // `token` can be used if the user requests the code be sent again, to
              // tie the two requests together.
          },
          codeAutoRetrievalTimeOut: (id) => {
              // Called when the auto-sms-retrieval has timed out, based on the given
              // timeout parameter.
              // `id` contains the verification id of the request that timed out.
          });
    }
    public void VerifyOtp()
    {

        print("verify otp pressed");
        string verificationId = PlayerPrefs.GetString("verificationId");
        string verificationCode = EnterCode_Inp.text;

        PhoneAuthCredential credential = provider.GetCredential(verificationId, verificationCode);

        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " +
                               task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.Log("User signed in successfully");
            showLogMsg("Success");
            // This should display the phone number.
            Debug.Log("Phone number: " + newUser.PhoneNumber);
            // The phone number providerID is 'phone'.
            Debug.Log("Phone provider ID: " + newUser.ProviderId);

            AllOptions.SetActive(false);
            SuccessUi.SetActive(true);
            SuccessUi.transform.Find("Desc").GetComponent<TextMeshProUGUI>().text = "Id: " + task.Result.User.UserId;

        });
    }

    #region extra
    void showLogMsg(string msg)
    {
        logTxt.text = msg;
        logTxt.GetComponent<Animation>().Play("textFadeout");
    }
    #endregion
}