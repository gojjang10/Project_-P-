using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class UserSaveData
{
    // 유저 기본 정보
    public string userId;
    public long gold;
    public int currentStage;

    // 스탯 레벨
    public int atkLevel;      // 공격력 레벨
    public int hpLevel;       // 체력 레벨
    public int hpRegenLevel;  // 체력 회복 레벨
    public int atkSpeedLevel; // 공격 속도 레벨

    // 오프라인 보상 계산용
    public string lastLogoutTime;

    // 기본값 생성자
    public UserSaveData()
    {
        userId = "NewUser";
        gold = 0;
        currentStage = 1;
        atkLevel = 1;
        hpLevel = 1;
        hpRegenLevel = 1;
        atkSpeedLevel = 1;
        lastLogoutTime = DateTime.UtcNow.ToString();
    }
}
