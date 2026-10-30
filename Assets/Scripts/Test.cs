using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Analytics;

public class Test : MonoBehaviour
{
    [SerializeField]
    private Button[] buttons;
    
    FirebaseAuth auth;
    
    void Start()
    {
        for(var i = 0; i < buttons.Length; i++)
        {
            var id = i;
            var button = buttons[i];
            button.onClick.AddListener(() => OnButtonClicked(id));
        }
        
        auth = FirebaseAuth.DefaultInstance;
    }

    /// <summary>
    /// ボタン コールバック処理
    /// </summary>
    private void OnButtonClicked(int id)
    {
        switch (id)
        {
            case 0:
            {
                Login();
            }
            break;

            case 1:
            {
                var parame = new Parameter[]
                {
                    new Parameter("test", "value1"),
                };
                LogEvent("test_event", parame);
            }
            break;
        }
    }

    /// <summary>
    /// ログイン処理
    /// </summary>
    private void Login()
    {
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            
            var userId = result.User.UserId;
            FirebaseAnalytics.SetUserId(userId);
            
            Debug.LogFormat($"User signed in successfully: {result.User.DisplayName} ({result.User.UserId})");
        });
    }

    /// <summary>
    /// ログイベントを送信する
    /// </summary>
    private void LogEvent(string lavel, Parameter[] parameter)
    {
        FirebaseAnalytics.LogEvent(lavel, parameter);
        Debug.LogFormat($"Event logged: {lavel}");
    }
    
}
