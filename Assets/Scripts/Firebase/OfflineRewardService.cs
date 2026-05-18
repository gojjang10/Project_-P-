using System;
using UnityEngine;

/// <summary>
/// lastLoginTime 기준 오프라인 골드 보상 계산.
/// DataManager.OnUserDataLoaded 이후 CheckAndApply() 호출.
/// </summary>
public class OfflineRewardService : MonoBehaviour
{
    [SerializeField] private long goldPerHour = 100;
    [SerializeField] private double maxOfflineHours = 24;

    private void OnEnable()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.OnUserDataLoaded += OnUserDataLoaded;
    }

    private void OnDisable()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.OnUserDataLoaded -= OnUserDataLoaded;
    }

    private void OnUserDataLoaded(UserSaveData data)
    {
        CheckAndApply(data);
    }

    public long CalculateOfflineGold(UserSaveData data)
    {
        if (data == null || string.IsNullOrEmpty(data.lastLoginTime))
            return 0;

        if (!DateTime.TryParse(data.lastLoginTime, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out var lastLogin))
            return 0;

        double hours = (DateTime.UtcNow - lastLogin.ToUniversalTime()).TotalHours;
        hours = Math.Min(hours, maxOfflineHours);

        if (hours <= 0)
            return 0;

        return (long)(hours * goldPerHour);
    }

    public void CheckAndApply(UserSaveData data)
    {
        long reward = CalculateOfflineGold(data);
        if (reward > 0)
        {
            DataManager.Instance.AddGold(reward);
            FirebaseAnalyticsHelper.LogOfflineRewardClaimed(reward,
                (DateTime.UtcNow - DateTime.Parse(data.lastLoginTime).ToUniversalTime()).TotalHours);
            Debug.Log($"[OfflineReward] 오프라인 보상: {reward} 골드");
        }

        DataManager.Instance.TouchLoginTime();
    }
}
