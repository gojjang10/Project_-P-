using UnityEngine;

/// <summary>
/// 플레이어(슬라임): 일정 주기로 씬의 Enemy를 찾아 공격합니다. 공격력은 atkLevel 기반.
/// </summary>
public class Player : MonoBehaviour, IDamagable
{
    [SerializeField] private float attackInterval = 1f;         // 공격 주기
    [SerializeField] private float baseDamage = 5f;             // 기본 공격력    
    [SerializeField] private float damagePerAtkLevel = 5f;      // 공격력 레벨 1당 증가하는 데미지 수치

    [Header("플레이어 HP (IDamagable)")]
    [SerializeField] private float maxHp = 100f;                // 플레이어 체력

    private float _attackTimer;     // 시간이 얼마나 흘렀는지 체크하는 타이머
    private float _currentHp;       // 플레이어 현재 체력
    private bool _dead;             // 플레이어 죽었는지 체크

    public float CurrentHp => _currentHp;

    private void Awake()
    {
        _currentHp = maxHp;
    }

    private void Update()
    {
        // 죽었으면 리턴
        if (_dead)
            return;

        // 공격 주기마다 공격 시도
        _attackTimer += Time.deltaTime;

        // 공격 주기가 아직 안 됐으면 리턴
        if (_attackTimer < attackInterval)
            return;

        // 공격 주기가 됐으면 타이머 초기화하고 공격 시도
        _attackTimer = 0f;

        // 가장 가까운 적을 찾아 공격
        TryAttackNearestEnemy();
    }

    // 씬에 존재하는 Enemy 오브젝트들을 찾아서 가장 가까운 적에게 ComputeDamageFromData()로 계산된 데미지를 입히는 함수
    private void TryAttackNearestEnemy()
    {
        // 씬에 존재하는 Enemy 오브젝트들을 모두 찾아서 배열로 반환 (비활성화된 오브젝트는 제외)
        var enemies = Object.FindObjectsByType<Enemy>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        // 적이 하나도 없으면 리턴
        if (enemies == null || enemies.Length == 0)
            return;
        // 가장 가까운 적에게 데미지 입히기 (현재는 그냥 첫 번째 적에게 공격)
        float damage = ComputeDamageFromData();
        enemies[0].TakeDamage(damage);
    }

    // 파이어베이스 데이터활용 로직: DataManager에서 UserData를 참조하여 atkLevel 기반으로 데미지를 계산하는 함수
    private float ComputeDamageFromData()
    {
        // DataManager가 존재하고 파이어베이스 데이터(UserData)가 잘 로드되었다면
        if (DataManager.Instance != null && DataManager.Instance.IsReady &&
            DataManager.Instance.UserData != null)
        {
            // 파이어베이스에서 atkLevel 수치 불러오기
            int atkLevel = DataManager.Instance.UserData.atkLevel;
            // atkLevel에 따라 증가하는 데미지 계산하여 반환
            return baseDamage + atkLevel * damagePerAtkLevel;
        }

        // 데이터가 없거나 로드 실패 시 기본 데미지 반환 (최소 1 이상)
        return Mathf.Max(1f, baseDamage);
    }

    public void TakeDamage(float damage)
    {
        if (_dead || damage <= 0f)
            return;

        _currentHp -= damage;
        if (_currentHp <= 0f)
            OnDefeated();
    }

    private void OnDefeated()
    {
        if (_dead)
            return;

        _dead = true;
        enabled = false;
        Debug.Log("[Slime] HP가 0이 되어 전투를 중지했습니다.");
    }
}
