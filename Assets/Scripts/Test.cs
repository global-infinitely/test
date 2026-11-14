using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Messaging;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#elif UNITY_IOS && !UNITY_EDITOR
using Unity.Notifications.iOS;
using System.Runtime.InteropServices;
#endif

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

        InitializeFirebaseMessaging();
        
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
#elif UNITY_IOS && !UNITY_EDITOR
        RequestIOSNotificationPermission();
#endif
    }
    
    /// <summary>
    /// iOSの通知許可をリクエストする
    /// </summary>
    private void RequestIOSNotificationPermission()
    {
        // iOS向けの通知許可をリクエスト
        Firebase.Messaging.FirebaseMessaging.RequestPermissionAsync().ContinueWith(task =>
        {
            bool granted = ((System.Threading.Tasks.Task<bool>)task).Result;
            if (granted)
            {
                Debug.Log("iOSの通知権限が許可されました。");
            }
            else
            {
                Debug.Log("iOSの通知権限が拒否されました。");
            }
        });
    }
    
    /// <summary>
    /// イベント解除
    /// </summary>
    private void OnDestroy()
    {
        FirebaseMessaging.TokenReceived -= OnTokenReceived;
        FirebaseMessaging.MessageReceived -= OnMessageReceived;
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

            case 2:
            {
                CrashLog("Test crash log");
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
            Crashlytics.SetUserId(userId);
            
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
    
    /// <summary>
    /// クラッシュログを送信する
    /// </summary>
    private void CrashLog(string message)
    {
        Crashlytics.LogException(new System.Exception(message));
    }
    
    /// <summary>
    /// Firebase Cloud Messaging の初期化とイベント登録
    /// </summary>
    private void InitializeFirebaseMessaging()
    {
        // イベントハンドラの登録
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    /// <summary>
    /// デバイストークン取得時のイベントハンドラ
    /// </summary>
    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        // コンソールやLogcatでトークンを確認できるように出力
        Debug.Log($"[FCM] デバイストークンを受信しました: {token.Token}");
    }

    /// <summary>
    /// メッセージ（プッシュ通知）受信時のイベントハンドラ
    /// </summary>
    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log($"[FCM] メッセージを受信しました: {e.Message.From}");

        // 通知データの詳細を取得
        if (e.Message.Notification != null)
        {
            string title = e.Message.Notification.Title;
            string body = e.Message.Notification.Body;
            Debug.Log($"[FCM] {title} | {body}");
#if UNITY_IOS && !UNITY_EDITOR
            // フォアグラウンド時にOSバナーが出ないため、ローカル通知で強制的にバナーを表示する
            var notification = new iOSNotification()
            {
                Identifier = "fcm_foreground_notification",
                Title = title,
                Body = body,
                ShowInForeground = true, // フォアグラウンド表示を許可
                ForegroundPresentationOption = (PresentationOption)(PresentationOption.Alert | PresentationOption.Sound | PresentationOption.Badge),
                Trigger = new iOSNotificationTimeIntervalTrigger()
                {
                    TimeInterval = new System.TimeSpan(0, 0, 1), // 1秒後に即時発火
                    Repeats = false
                }
            };

            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }

        // カスタムデータ（データメッセージ）が含まれる場合
        if (e.Message.Data.Count > 0)
        {
            foreach (var pair in e.Message.Data)
            {
                Debug.Log($"[FCM] データ: {pair.Key} = {pair.Value}");
            }
        }
    }
    
}
