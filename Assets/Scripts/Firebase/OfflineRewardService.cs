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

    private void Start()
    {
        // Start는 모든 스크립트의 Awake가 끝난 뒤에 실행되므로 Instance가 존재
        if (DataManager.Instance != null)
        {
            // 만약 내가 구독하기 전에 이미 데이터 로드가 빛의 속도로 끝났다면?
            if (DataManager.Instance.IsReady)
            {
                // 바로 보상 계산 실행
                CheckAndApply(DataManager.Instance.UserData);
            }
            else
            {
                // 아직 로드 중이라면 로드 완료 이벤트에 구독
                DataManager.Instance.OnUserDataLoaded += OnUserDataLoaded;
            }
        }
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
