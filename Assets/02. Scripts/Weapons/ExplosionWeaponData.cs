using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 폭발 무기 전용 기본값. 쿨다운과 사거리는 기존 스탯을 그대로 쓰고, 폭발 범위만 새 스탯이다.
    [CreateAssetMenu(fileName = "ExplosionWeaponData", menuName = "FiveMinuteHero/Weapons/Explosion Weapon Data")]
    public class ExplosionWeaponData : WeaponData
    {
        // 쿨다운이 0이 되면 매 프레임 폭발하게 된다.
        private const float MinCooldown = 0.05f;
        private const float MinRange = 0.1f;
        // 반경이 0이면 폭발 지점의 적도 맞지 않을 수 있고, 이펙트도 보이지 않는다.
        private const float MinExplosionRadius = 0.1f;

        [SerializeField, Min(MinCooldown)] private float _cooldown = 2.5f;              // 폭발 간격(초)
        [SerializeField, Min(MinRange)] private float _range = 6f;                      // 목표로 고를 수 있는 적까지의 거리(유닛)
        [SerializeField, Min(MinExplosionRadius)] private float _explosionRadius = 1.5f; // 폭발 지점에서 피해가 닿는 거리(유닛)

        protected override void DefineStats(WeaponRuntimeStats stats)
        {
            stats.Define(WeaponStatType.Cooldown, _cooldown, MinCooldown);
            stats.Define(WeaponStatType.Range, _range, MinRange);
            stats.Define(WeaponStatType.ExplosionRadius, _explosionRadius, MinExplosionRadius);
        }
    }
}
