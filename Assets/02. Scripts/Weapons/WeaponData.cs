using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 무기 1종의 수치와 투사체 프리팹. 밸런스·외형 교체는 이 에셋만 바꾸면 된다.
    [CreateAssetMenu(fileName = "WeaponData", menuName = "FiveMinuteHero/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField, Min(0)] private int _damage = 5;
        [SerializeField, Min(0.05f)] private float _cooldown = 1f;        // 발사 간격(초)
        [SerializeField, Min(0.1f)] private float _projectileSpeed = 8f;  // 초당 이동 거리(유닛)
        [SerializeField, Min(0.1f)] private float _range = 6f;            // 조준 거리이자 투사체 비행 거리(유닛)
        [SerializeField] private Projectile _projectilePrefab;

        public int Damage => _damage;
        public float Cooldown => _cooldown;
        public float ProjectileSpeed => _projectileSpeed;
        public float Range => _range;
        public Projectile ProjectilePrefab => _projectilePrefab;
    }
}
