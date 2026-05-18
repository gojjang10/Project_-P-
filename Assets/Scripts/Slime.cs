using UnityEngine;

/// <summary>
/// 플레이어(슬라임): 일정 주기로 씬의 Enemy를 찾아 공격합니다. 공격력은 atkLevel 기반.
/// </summary>
public class Slime : MonoBehaviour, IDamagable
{
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float baseDamage = 5f;
    [SerializeField] private float damagePerAtkLevel = 5f;

    [Header("플레이어 HP (IDamagable)")]
    [SerializeField] private float maxHp = 100f;

    private float _attackTimer;
    private float _currentHp;
    private bool _dead;

    public float CurrentHp => _currentHp;

    private void Awake()
    {
        _currentHp = maxHp;
    }

    private void Update()
    {
        if (_dead)
            return;

        _attackTimer += Time.deltaTime;
        if (_attackTimer < attackInterval)
            return;

        _attackTimer = 0f;
        TryAttackNearestEnemy();
    }

    private void TryAttackNearestEnemy()
    {
        var enemies = Object.FindObjectsByType<Enemy>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        if (enemies == null || enemies.Length == 0)
            return;

        float damage = ComputeDamageFromData();
        enemies[0].TakeDamage(damage);
    }

    private float ComputeDamageFromData()
    {
        if (DataManager.Instance != null && DataManager.Instance.IsReady &&
            DataManager.Instance.UserData != null)
        {
            int atkLevel = DataManager.Instance.UserData.atkLevel;
            return baseDamage + atkLevel * damagePerAtkLevel;
        }

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
