using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    // 현재 유저의 실시간 데이터
    public UserSaveData UserData;

    private void Awake()
    {
        // 싱글톤 세팅
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadGameData();
    }

    // 데이터 저장 (우선 로컬에 저장하고, 곧 파이어베이스로 확장 예정)
    public void SaveGameData()
    {
        UserData.lastLogoutTime = DateTime.UtcNow.ToString();
        string json = JsonUtility.ToJson(UserData);
        PlayerPrefs.SetString("UserSaveData", json);
        PlayerPrefs.Save();
        Debug.Log("데이터 로컬 저장 완료: " + json);
    }

    // 데이터 로드
    public void LoadGameData()
    {
        if (PlayerPrefs.HasKey("UserSaveData"))
        {
            string json = PlayerPrefs.GetString("UserSaveData");
            UserData = JsonUtility.FromJson<UserSaveData>(json);
        }
        else
        {
            // 데이터가 없으면 새로 생성
            UserData = new UserSaveData();
        }
    }
}
