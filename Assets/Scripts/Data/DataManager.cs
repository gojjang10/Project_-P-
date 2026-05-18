
using System;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

/// <summary>
/// 유저 세이브 데이터 싱글톤. Firebase 익명 로그인 후 RTDB에서 로드/저장.
/// 씬에 빈 GameObject를 만들고 이 컴포넌트를 붙여 사용하세요.
/// </summary>
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public UserSaveData UserData { get; private set; }
    public bool IsReady { get; private set; }
    public string CurrentUserId { get; private set; }

    public event Action<UserSaveData> OnUserDataLoaded;
    public event Action<long> OnGoldChanged;

    private const string UsersRoot = "users";

    private FirebaseAuth auth;
    private DatabaseReference userRef;
    private bool isSaving;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeFirebase();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && IsReady)
            SaveAllAsync();
    }

    private void OnApplicationQuit()
    {
        if (IsReady)
            SaveAllAsync();
    }

    #region Firebase 초기화 · 로그인 · 로드

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[DataManager] Firebase 의존성 확인 실패: {task.Exception}");
                return;
            }

            if (task.Result != DependencyStatus.Available)
            {
                Debug.LogError($"[DataManager] Firebase 사용 불가: {task.Result}");
                return;
            }

            auth = FirebaseAuth.DefaultInstance;

            // [중요!] 이미 로그인된 유저(CurrentUser)가 있는지 먼저 확인해
            if (auth.CurrentUser != null)
            {
                CurrentUserId = auth.CurrentUser.UserId;
                userRef = FirebaseDatabase.DefaultInstance.GetReference(UsersRoot).Child(CurrentUserId);

                Debug.Log($"[DataManager] 기존 세션 발견! UID: {CurrentUserId}");
                LoadUserData(); // 바로 데이터 로드로 넘어감
            }
            else
            {
                // 없으면 그때만 익명 로그인을 새로 시도함
                SignInAnonymously();
            }
        });
    }

    private void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("[DataManager] 익명 로그인 취소됨");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError($"[DataManager] 익명 로그인 실패: {task.Exception}");
                return;
            }

            var user = task.Result.User;
            CurrentUserId = user.UserId;
            userRef = FirebaseDatabase.DefaultInstance
                .GetReference(UsersRoot)
                .Child(CurrentUserId);

            Debug.Log($"[DataManager] 익명 로그인 성공: {CurrentUserId}");
            LoadUserData();
        });
    }

    private void LoadUserData()
    {
        userRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[DataManager] 데이터 로드 실패: {task.Exception}");
                return;
            }

            var snapshot = task.Result;

            if (snapshot.Exists && snapshot.ChildrenCount > 0)
            {
                UserData = SnapshotToUserData(snapshot);
                UserData.userId = CurrentUserId;
                Debug.Log("[DataManager] 서버 데이터 로드 완료");
            }
            else
            {
                UserData = UserSaveData.CreateDefault(CurrentUserId);
                Debug.Log("[DataManager] 신규 유저 — 초기 데이터 생성");
                SaveAllAsync();
            }

            IsReady = true;
            OnUserDataLoaded?.Invoke(UserData);
        });
    }

    private static UserSaveData SnapshotToUserData(DataSnapshot snapshot)
    {
        return new UserSaveData
        {
            userId = GetString(snapshot, "userId"),
            gold = GetLong(snapshot, "gold"),
            currentStage = GetInt(snapshot, "currentStage", 1),
            atkLevel = GetInt(snapshot, "atkLevel", 1),
            hpLevel = GetInt(snapshot, "hpLevel", 1),
            lastLoginTime = GetString(snapshot, "lastLoginTime")
        };
    }

    private static string GetString(DataSnapshot snapshot, string key)
    {
        return snapshot.Child(key).Exists ? snapshot.Child(key).Value?.ToString() ?? "" : "";
    }

    private static long GetLong(DataSnapshot snapshot, string key, long defaultValue = 0)
    {
        if (!snapshot.Child(key).Exists || snapshot.Child(key).Value == null)
            return defaultValue;

        return Convert.ToInt64(snapshot.Child(key).Value);
    }

    private static int GetInt(DataSnapshot snapshot, string key, int defaultValue = 0)
    {
        if (!snapshot.Child(key).Exists || snapshot.Child(key).Value == null)
            return defaultValue;

        return Convert.ToInt32(snapshot.Child(key).Value);
    }

    #endregion

    #region 골드 · 스탯 · 전체 저장

    /// <summary>골드만 서버에 비동기 저장.</summary>
    public void SaveGold()
    {
        if (!CanSave()) return;

        userRef.Child("gold").SetValueAsync(UserData.gold).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
                Debug.LogError($"[DataManager] SaveGold 실패: {task.Exception}");
        });
    }

    public void SetGold(long gold)
    {
        if (!IsReady || UserData == null) return;

        UserData.gold = gold;
        OnGoldChanged?.Invoke(UserData.gold);
        SaveGold();
    }

    public void AddGold(long amount)
    {
        if (!IsReady || UserData == null || amount == 0) return;

        UserData.gold += amount;
        OnGoldChanged?.Invoke(UserData.gold);
        SaveGold();
    }

    public void SaveStage()
    {
        if (!CanSave()) return;

        userRef.Child("currentStage").SetValueAsync(UserData.currentStage).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
                Debug.LogError($"[DataManager] SaveStage 실패: {task.Exception}");
        });
    }

    public void SaveStats()
    {
        if (!CanSave()) return;

        var updates = new System.Collections.Generic.Dictionary<string, object>
        {
            { "atkLevel", UserData.atkLevel },
            { "hpLevel", UserData.hpLevel }
        };

        userRef.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
                Debug.LogError($"[DataManager] SaveStats 실패: {task.Exception}");
        });
    }

    /// <summary>전체 유저 데이터를 서버에 저장.</summary>
    public void SaveAllAsync()
    {
        if (!CanSave() || isSaving) return;

        isSaving = true;
        UserData.userId = CurrentUserId;
        UserData.lastLoginTime = DateTime.UtcNow.ToString("o");

        string json = JsonUtility.ToJson(UserData);
        userRef.SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            isSaving = false;
            if (task.IsFaulted)
                Debug.LogError($"[DataManager] SaveAll 실패: {task.Exception}");
            else
                Debug.Log("[DataManager] 전체 저장 완료");
        });
    }

    /// <summary>접속 시각 갱신 (오프라인 보상 계산 후 호출).</summary>
    public void TouchLoginTime()
    {
        if (!CanSave()) return;

        UserData.lastLoginTime = DateTime.UtcNow.ToString("o");
        userRef.Child("lastLoginTime").SetValueAsync(UserData.lastLoginTime);
    }

    private bool CanSave()
    {
        return IsReady && userRef != null && UserData != null;
    }

    #endregion
}

