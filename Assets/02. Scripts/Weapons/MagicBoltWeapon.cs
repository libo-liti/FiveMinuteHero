using System.Collections.Generic;
using FiveMinuteHero.Enemies;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 쿨타임마다 사거리 안에서 가장 가까운 적에게 마법탄을 쏜다.
    public class MagicBoltWeapon : MonoBehaviour
    {
        [SerializeField] private EnemyRegistry _registry;
        [SerializeField] private WeaponData _data;
        // 루트 원점이 발이라서, 몸 높이에 둔 자식 Transform에서 쏜다.
        [SerializeField] private Transform _firePoint;

        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < _data.Cooldown)
            {
                return;
            }

            Enemy target = FindNearestEnemy();
            if (target == null)
            {
                // 쿨타임이 찬 채로 기다리다가, 적이 사거리에 들어오는 즉시 쏜다.
                _timer = _data.Cooldown;
                return;
            }

            _timer -= _data.Cooldown;
            Fire(target);
        }

        // perf-baseline 전이라 살아 있는 적 전체를 단순 순회한다(O(n)).
        private Enemy FindNearestEnemy()
        {
            Vector2 origin = _firePoint.position;
            // 제곱 거리로 비교해서 매번 제곱근을 계산하지 않는다.
            float nearestSqrDistance = _data.Range * _data.Range;
            Enemy nearest = null;

            IReadOnlyList<Enemy> enemies = _registry.AliveEnemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                float sqrDistance = (enemies[i].AimPoint - origin).sqrMagnitude;
                if (sqrDistance <= nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = enemies[i];
                }
            }

            return nearest;
        }

        private void Fire(Enemy target)
        {
            Vector2 origin = _firePoint.position;
            Vector2 direction = (target.AimPoint - origin).normalized;

            // 플레이어를 따라 움직이지 않도록 부모 없이 만든다.
            Projectile projectile = Instantiate(_data.ProjectilePrefab, origin, Quaternion.identity);
            // 사거리만큼 날아가면 사라지도록 수명 = 거리 / 속도.
            projectile.Launch(direction, _data.ProjectileSpeed, _data.Damage, _data.Range / _data.ProjectileSpeed);
        }
    }
}
