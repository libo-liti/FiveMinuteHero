using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 회전 오브 전용 기본값. 쿨다운·사거리·투사체가 없고, 대신 오브 배치와 재타격 간격을 가진다.
    [CreateAssetMenu(fileName = "OrbitWeaponData", menuName = "FiveMinuteHero/Weapons/Orbit Weapon Data")]
    public class OrbitWeaponData : WeaponData
    {
        // 오브가 0개면 무기가 없는 것과 같다.
        private const int MinOrbCount = 1;
        private const float MinRotationSpeed = 1f;
        // 반지름이 0이면 오브가 모두 플레이어 한 점에 겹친다.
        private const float MinOrbitRadius = 0.1f;
        // 재타격 간격이 0이면 닿아 있는 적을 매 프레임 때린다.
        private const float MinHitInterval = 0.05f;

        [SerializeField, Min(MinOrbCount)] private int _orbCount = 2;
        [SerializeField, Min(MinRotationSpeed)] private float _rotationSpeed = 180f; // 초당 회전 각도(도)
        [SerializeField, Min(MinOrbitRadius)] private float _orbitRadius = 1.5f;     // 플레이어 몸 중심에서 오브까지 거리(유닛)
        [SerializeField, Min(MinHitInterval)] private float _hitInterval = 0.5f;     // 같은 적을 다시 때리기까지 걸리는 시간(초)

        protected override void DefineStats(WeaponRuntimeStats stats)
        {
            stats.Define(WeaponStatType.OrbCount, _orbCount, MinOrbCount);
            stats.Define(WeaponStatType.RotationSpeed, _rotationSpeed, MinRotationSpeed);
            stats.Define(WeaponStatType.OrbitRadius, _orbitRadius, MinOrbitRadius);
            stats.Define(WeaponStatType.HitInterval, _hitInterval, MinHitInterval);
        }
    }
}
