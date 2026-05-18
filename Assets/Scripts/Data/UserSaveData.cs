using System;

/// <summary>
/// Firebase Realtime Database users/{uid}/ 에 저장되는 유저 세이브 데이터.
/// </summary>
[Serializable]
public class UserSaveData
{
    public string userId;
    public long gold;
    public int currentStage;
    public int atkLevel;
    public int hpLevel;

    /// <summary>오프라인 보상·마지막 접속 시각 (UTC ISO 8601)</summary>
    public string lastLoginTime;

    public static UserSaveData CreateDefault(string uid)
    {
        return new UserSaveData
        {
            userId = uid,
            gold = 0,
            currentStage = 1,
            atkLevel = 1,
            hpLevel = 1,
            lastLoginTime = DateTime.UtcNow.ToString("o")
        };
    }
}
