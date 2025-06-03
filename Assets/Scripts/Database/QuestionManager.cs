using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase;
using UnityEngine.UI;
using System.Collections;

public class QuestionManager : MonoBehaviour
{
    private DatabaseReference dbRef;
    private string userId ; // Replace with actual user ID
    
    private FirebaseAuth auth;
    string questionText;
    string qId;
    string qText;
    string correctAnswer;
    string QpostedBy;
    long prizePerPlay;
    long prizeTokens;
    List<string> allAnswers;
    [SerializeField] private ForYouPage forYouPage;
    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase initialized!");

                if (auth.CurrentUser != null)
                {
                    userId = auth.CurrentUser.UserId;
                    Debug.Log("Auto-login: User already signed in: " + userId);
                    
                }
                StartCoroutine(waitforQuestion());


            }
            else
            {
                Debug.LogError("Could not resolve Firebase dependencies: " + task.Result);
                
            }
        });
    }
    IEnumerator waitforQuestion()
    {
        yield return new WaitForSeconds(3);
        LoadForYou();
    }
    
    // 🔵 1. Post a question
    public void PostQuestion(string questionText, List<string> answers, int prizeTokens,string correctAnswer)
    {
        int postingCost = 3; // Fixed $3 cost
        int totalCost = postingCost + prizeTokens;

        dbRef.Child("users").Child(userId).Child("tokens").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled) return;
            int userTokens = int.Parse(task.Result.Value.ToString());

            if (userTokens < totalCost)
            {
                Debug.Log("Not enough tokens.");
                return;
            }

            string questionId = dbRef.Child("questions").Push().Key;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long expiry = now + 30L * 24 * 60 * 60 * 1000; // 30 days

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "userId", userId },
                { "postedBy", userId },
                { "questionText", questionText },
                { "answers", answers },
                { "correctAnswer", correctAnswer },
                { "prizeTokens", prizeTokens },
                { "pricePerPlay", prizeTokens },
                { "createdAt", now },
                { "expiresAt", expiry },
                { "viewedBy", new Dictionary<string, bool>() }
            };

            dbRef.Child("questions").Child(questionId).SetValueAsync(data).ContinueWithOnMainThread(postTask =>
            {
                if (!postTask.IsFaulted)
                {
                    dbRef.Child("users").Child(userId).Child("tokens").SetValueAsync(userTokens - totalCost);
                    Debug.Log("Question posted.");
                }
            });
        });
    }

   
    public void LoadForYou()
    {
        DatabaseReference db = FirebaseDatabase.DefaultInstance.RootReference;
        string userId = auth.CurrentUser?.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User is not authenticated.");
            return;
        }

        Debug.Log("Starting LoadForYou for user: " + userId);

        db.Child("questions").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error loading questions: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                db.Child("Users").Child(userId).Child("viewedQuestions").GetValueAsync().ContinueWithOnMainThread(viewTask =>
                {
                    if (viewTask.IsFaulted)
                    {
                        Debug.LogError("Error loading viewed questions: " + viewTask.Exception);
                    }
                    else if (viewTask.IsCompleted)
                    {
                        List<string> viewed = new List<string>();
                        if (viewTask.Result.Exists)
                        {
                            foreach (var q in viewTask.Result.Children)
                                viewed.Add(q.Key);
                        }

                        bool foundQuestion = false;

                        foreach (var q in snapshot.Children)
                        {
                            string questionId = q.Key;
                            string postedBy = q.Child("postedBy").Value?.ToString();

                            if (postedBy == userId)
                            {
                                continue;
                            }

                            if (!viewed.Contains(questionId))
                            {
                                 questionText = q.Child("questionText").Value?.ToString();
                                //extra see all value:
                                 qId = q.Key;
                                 qText = q.Child("questionText").Value?.ToString();
                                 correctAnswer = q.Child("correctAnswer").Value?.ToString();
                                 QpostedBy = q.Child("postedBy").Value?.ToString();
                                // createdAt = (long)q.Child("createdAt").Value;
                                // expiresAt = (long)q.Child("expiresAt").Value;
                                 prizePerPlay = (long)q.Child("pricePerPlay").Value;
                                 prizeTokens = (long)q.Child("prizeTokens").Value;

                                allAnswers = new List<string>();

                                if (q.Child("answers").Exists && q.Child("answers").ChildrenCount > 0)
                                {
                                    foreach (var answer in q.Child("answers").Children)
                                    {
                                        string ans = answer.Value?.ToString();
                                        if (!string.IsNullOrEmpty(ans))
                                        {
                                            allAnswers.Add(ans);
                                        }
                                    }
                                }
                                Debug.Log("Loaded Answers (non-null): " + string.Join(", ", allAnswers));

                                Debug.Log($"Question ID: {qId}");
                                Debug.Log($"Question Text: {qText}");
                                Debug.Log($"Correct Answer: {correctAnswer}");
                                Debug.Log($"Posted By: {QpostedBy}");
                                //Debug.Log($"Created At: {createdAt}");
                                //Debug.Log($"Expires At: {expiresAt}");
                                Debug.Log($"Prize Per Play: {prizePerPlay}");
                                Debug.Log($"Prize Tokens: {prizeTokens}");
                                Debug.Log("------------------------");
                                //extra end.
                                Debug.Log("Loaded For You Question: " + questionText);

                                db.Child("Users").Child(userId).Child("viewedQuestions").Child(questionId).SetValueAsync(true);
                                
                                //initializing for you page texts frontend.
                                forYouPage.initializeFeeAndPrize();
                                forYouPage.initializeQuestions();
                                foundQuestion = true;
                                break;
                            }
                        }

                        if (!foundQuestion)
                        {
                            forYouPage.initializeNoAnswer();
                            Debug.Log("No new questions available.");
                        }
                    }
                });
            }
        });
    }
   
// 🔵 3. Play question
    public void PlayQuestion(string questionId)
    {
        dbRef.Child("questions").Child(questionId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled) return;

            var q = (IDictionary<string, object>)task.Result.Value;
            int price = Convert.ToInt32(q["pricePerPlay"]);

            dbRef.Child("users").Child(userId).Child("tokens").GetValueAsync().ContinueWithOnMainThread(userTask =>
            {
                int tokens = int.Parse(userTask.Result.Value.ToString());
                if (tokens < price)
                {
                    Debug.Log("Not enough tokens.");
                    return;
                }

                dbRef.Child("users").Child(userId).Child("tokens").SetValueAsync(tokens - price);
                Debug.Log("Started game. Timer: 1 min.");
            });
        });
    }

    // 🔵 4. Extend Time
    public void ExtendTime()
    {
        dbRef.Child("users").Child(userId).Child("tokens").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            int tokens = int.Parse(task.Result.Value.ToString());
            if (tokens < 1)
            {
                Debug.Log("Not enough tokens.");
                return;
            }

            dbRef.Child("users").Child(userId).Child("tokens").SetValueAsync(tokens - 1);
            Debug.Log("Time extended (+30s).");
        });
    }

    // 🔵 5. Reduce Choices
    public void ReduceChoices(int cost)
    {
        dbRef.Child("users").Child(userId).Child("tokens").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            int tokens = int.Parse(task.Result.Value.ToString());
            if (tokens < cost)
            {
                Debug.Log("Not enough tokens.");
                return;
            }

            dbRef.Child("users").Child(userId).Child("tokens").SetValueAsync(tokens - cost);
            Debug.Log("Choices reduced, time +1min.");
        });
    }

    // 🔵 6. Submit Answer
    public void SubmitAnswer(string questionId, string userAnswer, string correctAnswer)
    {
        dbRef.Child("users").Child(userId).Child("tokens").GetValueAsync().ContinueWithOnMainThread(userTask =>
        {
            int tokens = int.Parse(userTask.Result.Value.ToString());

            dbRef.Child("questions").Child(questionId).GetValueAsync().ContinueWithOnMainThread(qTask =>
            {
                var q = (IDictionary<string, object>)qTask.Result.Value;
                int playCost = Convert.ToInt32(q["pricePerPlay"]);
                int prize = Convert.ToInt32(q["prizeTokens"]);
                string ownerId = q["userId"].ToString();

                if (userAnswer == correctAnswer)
                {
                    dbRef.Child("users").Child(userId).Child("tokens").SetValueAsync(tokens + prize);
                    dbRef.Child("questions").Child(questionId).Child("winnerId").SetValueAsync(userId);
                    Debug.Log("Correct! Prize awarded.");
                }
                else
                {
                    int refund = playCost / 2;
                    int toOwner = playCost - refund;

                    dbRef.Child("users").Child(userId).Child("tokens").SetValueAsync(tokens + refund);
                    dbRef.Child("users").Child(ownerId).Child("tokens").GetValueAsync().ContinueWithOnMainThread(ownerTask =>
                    {
                        int ownerTokens = int.Parse(ownerTask.Result.Value.ToString());
                        dbRef.Child("users").Child(ownerId).Child("tokens").SetValueAsync(ownerTokens + toOwner);
                        Debug.Log("Wrong. Half refunded. Half to owner.");
                    });
                }

                dbRef.Child("questions").Child(questionId).Child("viewedBy").Child(userId).SetValueAsync(true);
            });
        });
    }
    public int getQuestionPrize()
    {
        return int.Parse(prizePerPlay.ToString());
    }
    public string getQuestionText()
    {
        return questionText;
    }
    public int getAnswersCount()
    {
        if (allAnswers != null)
        {
            return allAnswers.Count;
        }
        return 0;
    }
    public string getCorrectAnswer()
    {
        if(correctAnswer!=null)
        return correctAnswer;
        return "";
    }
    public List<string> getAllAnswers()
    {
        return allAnswers;
    }
}
