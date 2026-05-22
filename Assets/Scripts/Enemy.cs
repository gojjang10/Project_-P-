using UnityEngine;

/// <summary>
/// 적 HP·보상 골드 관리. 처치 시 DataManager에 골드 반영 후 제거.
/// </summary>
public class Enemy : MonoBehaviour, IDamagable
{
    // [SerializeField]로 변수를 private으로 보호하면서도, 유니티 인스펙터 창에서는 수정할 수 있게 열음
    [SerializeField] private float maxHp = 100f;                // 적의 최대 체력
    [SerializeField] private long goldRewardOnKill = 10L;       // 적이 죽었을 때 줄 골드 (L은 long 타입이라는 뜻)

    private float _currentHp;   // 현재 체력 (게임 중에 계속 깎일 값)
    private bool _dead;         // 죽었는지 살았는지 체크하는 스위치 (중복 사망 방지용)

    // 프로퍼티(Property): 다른 스크립트에서 적의 현재 HP와 최대 HP를 '읽을 수만' 있게 설정
    public float CurrentHp => _currentHp;
    public float MaxHp => maxHp;

    private void Awake()
    {
        // 적이 처음 태어날 때(Awake), 현재 체력을 최대 체력.
        _currentHp = maxHp;
    }

    // IDamagable 인터페이스에서 요구하는 TakeDamage 함수 구현
    public void TakeDamage(float damage)
    {
        // 이미 죽었거나, 데미지가 0 이하라면 아무것도 하지 않고 그냥 리턴
        if (_dead || damage <= 0f)
            return;

        // 데미지를 현재 체력에서 깎음
        _currentHp -= damage;

        // 데미지를 받고 나서 체력이 0 이하가 되면 Die() 함수 호출
        if (_currentHp <= 0f)
            Die();
    }

    private void Die()
    {
        // 중복해서 죽는 것(골드가 두 번 들어오는 버그 등)을 막기 위해 체크
        if (_dead)
            return;

        _dead = true;

        // DataManager가 씬에 존재하고, 준비된 상태(IsReady)라면
        if (DataManager.Instance != null && DataManager.Instance.IsReady)
            DataManager.Instance.AddGold(goldRewardOnKill);

        // 역할을 다 한 오브젝트를 씬에서 제거
        Destroy(gameObject);
    }
}