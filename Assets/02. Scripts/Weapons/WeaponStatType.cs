namespace FiveMinuteHero.Weapons
{
    // 강화로 바꿀 수 있는 무기 스탯. 투사체 프리팹은 수치가 아니라 넣지 않는다.
    // 에셋에는 숫자로 저장되므로 새 항목은 항상 끝에 추가한다(PlayerStatType과 같은 이유).
    public enum WeaponStatType { Damage, Cooldown, ProjectileSpeed, Range }
}
