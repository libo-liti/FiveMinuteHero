using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 마법탄 전용 기본값. 이 파일은 예전 WeaponData.cs의 이름만 바꾼 것이라 .meta GUID가 같아서,
    // 기존 MagicBoltData.asset이 그대로 이 클래스로 연결되고 필드 이름이 같아 값도 유지된다.
    [CreateAssetMenu(fileName = "MagicBoltData", menuName = "FiveMinuteHero/Weapons/Magic Bolt Data")]
    public class MagicBoltData : WeaponData
    {
        // 쿨다운이 0이 되면 매 프레임 발사하게 된다.
        private const float MinCooldown = 0.05f;
        private const float MinProjectileSpeed = 0.1f;
        private const float MinRange = 0.1f;

        [SerializeField, Min(MinCooldown)] private float _cooldown = 1f;               // 발사 간격(초)
        [SerializeField, Min(MinProjectileSpeed)] private float _projectileSpeed = 8f; // 초당 이동 거리(유닛)
        [SerializeField, Min(MinRange)] private float _range = 6f;                     // 조준 거리이자 투사체 비행 거리(유닛)

        protected override void DefineStats(WeaponRuntimeStats stats)
        {
            stats.Define(WeaponStatType.Cooldown, _cooldown, MinCooldown);
            stats.Define(WeaponStatType.ProjectileSpeed, _projectileSpeed, MinProjectileSpeed);
            stats.Define(WeaponStatType.Range, _range, MinRange);
        }
    }
}
