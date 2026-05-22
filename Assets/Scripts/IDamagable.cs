/// <summary>
/// 데미지를 받을 수 있는 대상 공통 인터페이스.
/// </summary>
public interface IDamagable
{
    // 데미지를 받는 함수. 내용물({ })은 없고 껍데기만 존재
    // 실제 내용물은 이걸 사용하는 Enemy나 Player 스크립트에서 직접 구현.
    void TakeDamage(float damage);
}