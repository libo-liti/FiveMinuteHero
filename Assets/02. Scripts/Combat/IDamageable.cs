namespace FiveMinuteHero.Combat
{
    // 피해를 받을 수 있는 대상의 공통 계약. 때리는 쪽은 상대가 플레이어인지 적인지 몰라도 된다.
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }
}
