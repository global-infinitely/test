package com.company.test;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.os.Build;
import android.util.Log;
import androidx.core.app.NotificationCompat;

import com.google.firebase.messaging.FirebaseMessagingService;
import com.google.firebase.messaging.RemoteMessage;

public class MyFirebaseMessagingService extends FirebaseMessagingService {
    
    private static final String TAG = "MyFCM";
    private static final String CHANNEL_ID = "default_channel_id_v2";

    @Override
    public void onMessageReceived(RemoteMessage remoteMessage) {
        super.onMessageReceived(remoteMessage);

        String title = "お知らせ";
        String body = "新しい通知が届きました";
        String longBody = "";

        // 1. Notificationペイロードから値を取得する
        if (remoteMessage.getNotification() != null) {
            if (remoteMessage.getNotification().getTitle() != null) {
                title = remoteMessage.getNotification().getTitle();
            }
            if (remoteMessage.getNotification().getBody() != null) {
                body = remoteMessage.getNotification().getBody();
                longBody = body;
            }
        } 
        
        // 2. Dataペイロード（カスタムデータ）から取得する場合
        if (remoteMessage.getData().size() > 0) {
            if (remoteMessage.getData().containsKey("title")) {
                title = remoteMessage.getData().get("title");
            }
            if (remoteMessage.getData().containsKey("body")) {
                body = remoteMessage.getData().get("body");
            }
            if (remoteMessage.getData().containsKey("long_body")) {
                longBody = remoteMessage.getData().get("long_body");
            } else if (!longBody.isEmpty()) {
                longBody = body;
            }
        }

        showBigTextNotification(title, body, longBody);
    }

    private void showBigTextNotification(String title, String shortText, String longText) {
        NotificationManager notificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);

        // Android 8.0以降のための通知チャネル作成
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    CHANNEL_ID,
                    "ゲーム詳細通知",
                    NotificationManager.IMPORTANCE_HIGH
            );
            channel.setDescription("展開可能な詳細通知");
            if (notificationManager != null) {
                notificationManager.createNotificationChannel(channel);
            }
        }

        // BigTextStyle の設定
        NotificationCompat.BigTextStyle bigTextStyle = new NotificationCompat.BigTextStyle();
        if (longText != null && !longText.isEmpty()) {
            bigTextStyle.bigText(longText);
        } else {
            bigTextStyle.bigText(shortText);
        }

        // 通知の構築
        NotificationCompat.Builder builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .setSmallIcon(android.R.drawable.ic_dialog_info)
                .setContentTitle(title)
                .setContentText(shortText)
                .setStyle(bigTextStyle)
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setAutoCancel(true);

        if (notificationManager != null) {
            int notificationId = (int) System.currentTimeMillis();
            
            // 余分な記述を削除し、正しくnotifyを呼ぶ
            notificationManager.notify(notificationId, builder.build());
            
            Log.d(TAG, "通知の発行完了 ID: " + notificationId);
        }
    }
}
