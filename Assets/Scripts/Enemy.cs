using UnityEngine;

/// <summary>
/// 적 HP·보상 골드 관리. 처치 시 DataManager에 골드 반영 후 제거.
/// </summary>
public class Enemy : MonoBehaviour, IDamagable
{
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private long goldRewardOnKill = 10L;

    private float _currentHp;
    private bool _dead;

    public float CurrentHp => _currentHp;
    public float MaxHp => maxHp;

    private void Awake()
    {
        _currentHp = maxHp;
    }

    public void TakeDamage(float damage)
    {
        if (_dead || damage <= 0f)
            return;

        _currentHp -= damage;
        if (_currentHp <= 0f)
            Die();
    }

    private void Die()
    {
        if (_dead)
            return;

        _dead = true;

        if (DataManager.Instance != null && DataManager.Instance.IsReady)
            DataManager.Instance.AddGold(goldRewardOnKill);

        Destroy(gameObject);
    }
}