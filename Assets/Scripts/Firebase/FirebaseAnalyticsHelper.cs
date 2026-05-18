using UnityEngine;

#if FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif

/// <summary>
/// 방치형 슬라임 게임용 Firebase Analytics 이벤트 래퍼.
/// Analytics Unity 패키지가 없으면 로그만 출력합니다.
/// Scripting Define Symbols에 FIREBASE_ANALYTICS 추가 시 실제 전송.
/// </summary>
public static class FirebaseAnalyticsHelper
{
    public static void LogStageClear(int stage)
    {
#if FIREBASE_ANALYTICS
        FirebaseAnalytics.LogEvent("stage_clear", new Parameter("stage", stage));
#else
        Debug.Log($"[Analytics] stage_clear stage={stage}");
#endif
    }

    public static void LogUpgrade(string statType, int level)
    {
#if FIREBASE_ANALYTICS
        FirebaseAnalytics.LogEvent("stat_upgrade",
            new Parameter("stat_type", statType),
            new Parameter("level", level));
#else
        Debug.Log($"[Analytics] stat_upgrade {statType}={level}");
#endif
    }

    public static void LogGoldEarned(long amount, string source)
    {
#if FIREBASE_ANALYTICS
        FirebaseAnalytics.LogEvent("gold_earned",
            new Parameter("amount", amount),
            new Parameter("source", source));
#else
        Debug.Log($"[Analytics] gold_earned {amount} from {source}");
#endif
    }

    public static void LogOfflineRewardClaimed(long gold, double offlineHours)
    {
#if FIREBASE_ANALYTICS
        FirebaseAnalytics.LogEvent("offline_reward",
            new Parameter("gold", gold),
            new Parameter("hours", offlineHours));
#else
        Debug.Log($"[Analytics] offline_reward gold={gold} hours={offlineHours:F1}");
#endif
    }

    public static void SetUserProperty(string name, string value)
    {
#if FIREBASE_ANALYTICS
        FirebaseAnalytics.SetUserProperty(name, value);
#else
        Debug.Log($"[Analytics] user_property {name}={value}");
#endif
    }
}
