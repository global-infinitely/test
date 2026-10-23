using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;

public class Test : MonoBehaviour
{
    [SerializeField] private Button loginButton;
    FirebaseAuth auth;
    
    void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        auth = FirebaseAuth.DefaultInstance;
    }

    private void OnLoginButtonClicked()
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
            Debug.LogFormat($"User signed in successfully: {result.User.DisplayName} ({result.User.UserId})");
        });
    }
    
}
