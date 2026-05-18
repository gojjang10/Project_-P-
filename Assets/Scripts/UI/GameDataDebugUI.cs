using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발용 — 로드된 유저 데이터를 화면에 표시. 릴리즈 빌드에서는 비활성화 권장.
/// </summary>
public class GameDataDebugUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;

    private void OnEnable()
    {
        if (DataManager.Instance != null)
        {
            if (DataManager.Instance.IsReady)
                Refresh(DataManager.Instance.UserData);
            else
                DataManager.Instance.OnUserDataLoaded += Refresh;
        }
    }

    private void OnDisable()
    {
        if (DataManager.Instance != null)
            DataManager.Instance.OnUserDataLoaded -= Refresh;
    }

    private void Refresh(UserSaveData data)
    {
        if (statusText == null || data == null) return;

        statusText.text =
            $"UID: {data.userId}\n" +
            $"Gold: {data.gold:N0}\n" +
            $"Stage: {data.currentStage}\n" +
            $"ATK Lv: {data.atkLevel}  HP Lv: {data.hpLevel}";
    }
}
