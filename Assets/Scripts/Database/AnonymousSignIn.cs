using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;

public class AnonymousSignIn : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField dayInput;
    [SerializeField] private TMP_InputField monthInput;
    [SerializeField] private TMP_InputField yearInput;
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private GameObject signUpSection, homePage;
    [SerializeField] private Button signUpButton;
    [SerializeField] private TMP_Text totalTokens;
    [SerializeField] private Text totalSpinText;
    private int totSpinRemain;
    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    private string currentUserId;
    private UserProfileData currentUserProfile;

    void Start()
    {
        signUpButton.onClick.AddListener(OnSignUpButtonClicked);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase initialized!");

                if (auth.CurrentUser != null)
                {
                    currentUserId = auth.CurrentUser.UserId;
                    Debug.Log("Auto-login: User already signed in: " + currentUserId);
                    debugText.text = "Auto-login as: " + currentUserId;
                    signUpSection.SetActive(false);
                    homePage.SetActive(true);
                    LoadUserData(currentUserId);
                    LoadTotalSpin();
                }
                else
                {
                    signUpSection.SetActive(true);
                    homePage.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("Could not resolve Firebase dependencies: " + task.Result);
                debugText.text = "Firebase error: " + task.Result;
            }
        });
    }

    public void OnSignUpButtonClicked()
    {
        string username = usernameInput.text.Trim();
        string day = dayInput.text.Trim();
        string month = monthInput.text.Trim();
        string year = yearInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(day) || string.IsNullOrEmpty(month) || string.IsNullOrEmpty(year))
        {
            Debug.LogWarning("Please fill all fields.");
            debugText.text = "Please fill all fields.";
            return;
        }

        CheckUsernameUnique(username, (isUnique) => {
            if (isUnique)
            {
                SignInAnonymouslyAndSave(username, day, month, year);
            }
            else
            {
                debugText.text = "Username is already taken. Please choose another.";
                Debug.LogWarning("Username is already taken. Please choose another.");
            }
        });
    }

    private void SignInAnonymouslyAndSave(string username, string day, string month, string year)
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(authTask => {
            if (authTask.IsCanceled || authTask.IsFaulted)
            {
                debugText.text = "Anonymous sign-in failed: " + authTask.Exception;
                Debug.LogWarning("Anonymous sign-in failed: " + authTask.Exception);
                return;
            }

            FirebaseUser user = authTask.Result.User;
            currentUserId = user.UserId;
            Debug.Log("Signed in anonymously with UID: " + currentUserId);
            debugText.text = "Signed in anonymously with UID: " + currentUserId;

            // Give 5 starting tokens and generate referral code
            string referralCode = GenerateReferralCode();
            int startingTokens = 5;
            int startingSpin = 0;
            currentUserProfile = new UserProfileData(username, day, month, year, startingTokens, referralCode, startingSpin);
            SaveUserData(currentUserId, currentUserProfile);

            signUpSection.SetActive(false);
            homePage.SetActive(true);
        });
    }

    private void SaveUserData(string uid, UserProfileData profile)
    {
        string json = JsonUtility.ToJson(profile);

        dbRef.Child("users").Child(uid).SetRawJsonValueAsync(json).ContinueWithOnMainThread(saveTask => {
            if (saveTask.IsCanceled || saveTask.IsFaulted)
            {
                debugText.text = "Failed to save user data: " + saveTask.Exception;
                Debug.LogError("Failed to save user data: " + saveTask.Exception);
            }
            else
            {
                debugText.text = $"Authentication complete! User data saved.\nTokens: {profile.tokens}\nReferral Code: {profile.referralCode}";
                Debug.Log("Authentication complete. User data saved successfully!");
            }
        });
    }

    private void LoadUserData(string uid)
    {
        dbRef.Child("users").Child(uid).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                debugText.text = "Failed to load user data: " + task.Exception;
                Debug.LogError("Failed to load user data: " + task.Exception);
            }
            else if (task.Result.Exists)
            {
                var json = task.Result.GetRawJsonValue();
                currentUserProfile = JsonUtility.FromJson<UserProfileData>(json);
                LoadTokens();
                debugText.text = $"Welcome back, {currentUserProfile.username}!\nTokens: {currentUserProfile.tokens}\nReferral Code: {currentUserProfile.referralCode}";
                Debug.Log($"Loaded user profile: {currentUserProfile.username}");
                Debug.Log($"Loaded user profile: {currentUserProfile.referralCode}");
            }
            else
            {
                debugText.text = "No user data found.";
                Debug.LogWarning("No user data found for UID: " + uid);
            }
        });
    }

    public void LoadTotalSpin()
    {
        dbRef.Child("users").Child(currentUserId).Child("totalSpin").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Failed to load totalSpin: " + task.Exception);
                debugText.text = "Failed to load totalSpin: " + task.Exception;
            }
            else if (task.Result.Exists)
            {
                int spins = int.Parse(task.Result.Value.ToString());
                totSpinRemain = spins;
                currentUserProfile.totalSpin = spins;
                debugText.text = $"Total spins loaded: {spins}";
                totalSpinText.text = spins.ToString();
                Debug.Log($"Total spins loaded: {spins}");
                
            }
            else
            {
                Debug.LogWarning("No totalSpin data found.");
                debugText.text = "No totalSpin data found.";
            }
        });
    }

    public void AddSpin(int amount)
    {
        int newTotal = currentUserProfile.totalSpin + amount;
        UpdateTotalSpin(newTotal);
    }
    public void RemoveSpin(int amount)
    {
        int newTotal = Mathf.Max(0, currentUserProfile.totalSpin - amount);
        UpdateTotalSpin(newTotal);
    }
    private void UpdateTotalSpin(int newTotal)
    {
        dbRef.Child("users").Child(currentUserId).Child("totalSpin").SetValueAsync(newTotal).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Failed to update totalSpin: " + task.Exception);
                debugText.text = "Failed to update totalSpin: " + task.Exception;
            }
            else
            {
                currentUserProfile.totalSpin = newTotal;
                debugText.text = $"Total spins updated! New total: {newTotal}";
                LoadTotalSpin();
                Debug.Log($"Total spins updated! New total: {newTotal}");
            }
        });
    }


    private void CheckUsernameUnique(string username, System.Action<bool> callback)
    {
        dbRef.Child("users").OrderByChild("username").EqualTo(username).GetValueAsync().ContinueWithOnMainThread(checkTask => {
            if (checkTask.IsCanceled || checkTask.IsFaulted)
            {
                debugText.text = "Failed to check username uniqueness: " + checkTask.Exception;
                Debug.LogError("Failed to check username uniqueness: " + checkTask.Exception);
                callback(false);
            }
            else
            {
                DataSnapshot snapshot = checkTask.Result;
                bool isUnique = !snapshot.Exists;
                callback(isUnique);
            }
        });
    }

    // Utility to generate random referral code (e.g., 6 uppercase letters/digits)
    private string GenerateReferralCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new System.Random();
        char[] code = new char[6];
        for (int i = 0; i < code.Length; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }
        return new string(code);
    }
    public void LoadTokens()
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogWarning("No user signed in. Cannot load tokens.");
            debugText.text = "No user signed in. Cannot load tokens.";
            return;
        }

        dbRef.Child("users").Child(currentUserId).Child("tokens").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Failed to load tokens: " + task.Exception);
                debugText.text = "Failed to load tokens: " + task.Exception;
            }
            else if (task.Result.Exists)
            {
                int tokens = int.Parse(task.Result.Value.ToString());
                currentUserProfile.tokens = tokens;
                debugText.text = $"Tokens loaded successfully! Current tokens: {tokens}";
                totalTokens.text = tokens.ToString();
                Debug.Log($"Tokens loaded successfully! Current tokens: {tokens}");
            }
            else
            {
                Debug.LogWarning("No tokens data found for this user.");
                debugText.text = "No tokens data found for this user.";
            }
        });
    }
    public void Logout()
    {
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
            Debug.Log("User signed out.");
            debugText.text = "You have been logged out.";
        }
        else
        {
            Debug.LogWarning("No user is currently signed in.");
            debugText.text = "No user is currently signed in.";
        }

        // Switch back to sign-in section
        signUpSection.SetActive(true);
        homePage.SetActive(false);
    }

    // Call this to add tokens
    public void AddTokens(int amount)
    {
        if (currentUserProfile == null) return;

        currentUserProfile.tokens += amount;
        SaveUserData(currentUserId, currentUserProfile);
        LoadTokens();
        debugText.text = $"Tokens updated! Current tokens: {currentUserProfile.tokens}";
        Debug.Log($"Tokens updated! Current tokens: {currentUserProfile.tokens}");
    }

    // Call this to remove tokens
    public void RemoveTokens(int amount)
    {
        if (currentUserProfile == null) return;

        currentUserProfile.tokens = Mathf.Max(0, currentUserProfile.tokens - amount);
        SaveUserData(currentUserId, currentUserProfile);
        LoadTokens();
        debugText.text = $"Tokens updated! Current tokens: {currentUserProfile.tokens}";
        Debug.Log($"Tokens updated! Current tokens: {currentUserProfile.tokens}");
    }
    public int getTotalSpin()
    {
        return totSpinRemain;
    }
}

[System.Serializable]
public class UserProfileData
{
    public string username;
    public string day;
    public string month;
    public string year;
    public int tokens;
    public string referralCode;
    public int totalSpin;

    public UserProfileData(string username, string day, string month, string year, int tokens, string referralCode, int totalSpin)
    {
        this.username = username;
        this.day = day;
        this.month = month;
        this.year = year;
        this.tokens = tokens;
        this.referralCode = referralCode;
        this.totalSpin = totalSpin;
    }
}

