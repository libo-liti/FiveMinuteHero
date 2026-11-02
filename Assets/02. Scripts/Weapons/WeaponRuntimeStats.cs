using FiveMinuteHero.Stats;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 플레이 중의 무기 수치. WeaponData SO를 기본값으로 쓰고, 강화 보너스는 여기에만 쌓는다(이유는 PlayerRuntimeStats와 같다).
    public class WeaponRuntimeStats
    {
        // 최솟값은 WeaponData의 [Min]과 맞춘다. 쿨다운이 0이 되면 매 프레임 발사하게 된다.
        private const float MinDamage = 0f;
        private const float MinCooldown = 0.05f;
        private const float MinProjectileSpeed = 0.1f;
        private const float MinRange = 0.1f;

        private readonly StatValue _damage;
        private readonly StatValue _cooldown;
        private readonly StatValue _projectileSpeed;
        private readonly StatValue _range;
        private readonly Projectile _projectilePrefab; // 수치가 아니라서 SO 값을 그대로 쓴다

        public WeaponRuntimeStats(WeaponData source)
        {
            _damage = new StatValue(source.Damage, MinDamage);
            _cooldown = new StatValue(source.Cooldown, MinCooldown);
            _projectileSpeed = new StatValue(source.ProjectileSpeed, MinProjectileSpeed);
            _range = new StatValue(source.Range, MinRange);
            _projectilePrefab = source.ProjectilePrefab;
        }

        // 피해량은 정수라서 % 보너스로 생긴 소수는 반올림한다.
        public int Damage => Mathf.RoundToInt(_damage.Value);
        public float Cooldown => _cooldown.Value;
        public float ProjectileSpeed => _projectileSpeed.Value;
        public float Range => _range.Value;
        public Projectile ProjectilePrefab => _projectilePrefab;
    }
}
