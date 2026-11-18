using System.Collections.Generic;
using FiveMinuteHero.Enemies;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 쿨타임마다 사거리 안에서 가장 가까운 적에게 마법탄을 쏜다.
    // 주입과 주입 확인은 Weapon이, 쿨타임 흐름은 CooldownWeapon이 맡는다. 여기는 "한 번 쏘기"만 정한다.
    public class MagicBoltWeapon : CooldownWeapon
    {
        // 수치가 아니라 이 무기의 외형이라서 런타임 스탯이 아니라 무기 쪽이 들고 있다.
        [SerializeField] private Projectile _projectilePrefab;

        // 런타임 스탯은 어떤 무기인지 모르므로, 마법탄이 쓰는 스탯을 여기서 이름 붙여 꺼낸다.
        // 쿨다운은 CooldownWeapon이 직접 읽으므로 여기에 없다.
        private float ProjectileSpeed => Stats.Get(WeaponStatType.ProjectileSpeed);
        private float Range => Stats.Get(WeaponStatType.Range);

        // 사거리 안에 적이 없으면 아무것도 쏘지 않고 false. CooldownWeapon이 쿨타임이 찬 채로 기다린다.
        protected override bool TryAttack()
        {
            Enemy target = FindNearestEnemy();
            if (target == null)
            {
                return false;
            }

            Fire(target);
            return true;
        }

        // perf-baseline 전이라 살아 있는 적 전체를 단순 순회한다(O(n)).
        private Enemy FindNearestEnemy()
        {
            // 이 무기는 플레이어의 WeaponMount(몸 높이) 아래에 생성되므로 자기 위치가 곧 발사 위치다.
            Vector2 origin = transform.position;
            // 제곱 거리로 비교해서 매번 제곱근을 계산하지 않는다.
            float nearestSqrDistance = Range * Range;
            Enemy nearest = null;

            IReadOnlyList<Enemy> enemies = Registry.AliveEnemies;
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
            Vector2 origin = transform.position;
            Vector2 direction = (target.AimPoint - origin).normalized;

            // 플레이어를 따라 움직이지 않도록 부모 없이 만든다.
            Projectile projectile = Instantiate(_projectilePrefab, origin, Quaternion.identity);
            // 사거리만큼 날아가면 사라지도록 수명 = 거리 / 속도.
            projectile.Launch(direction, ProjectileSpeed, Stats.Damage, Range / ProjectileSpeed);
        }
    }
}
