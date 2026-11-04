namespace FiveMinuteHero.Weapons
{
    // 강화로 바꿀 수 있는 무기 스탯 전체. 각 무기는 이 중 일부만 가진다(WeaponData.DefineStats가 정한다).
    // 무기가 갖지 않은 스탯을 강화하려 하면 WeaponRuntimeStats가 LogError로 막는다.
    // 에셋에는 숫자로 저장되므로 새 항목은 항상 끝에 추가한다(PlayerStatType과 같은 이유).
    public enum WeaponStatType { Damage, Cooldown, ProjectileSpeed, Range, OrbCount, RotationSpeed, OrbitRadius, HitInterval }
}
