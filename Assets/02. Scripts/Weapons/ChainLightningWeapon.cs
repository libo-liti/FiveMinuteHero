using System.Collections.Generic;
using FiveMinuteHero.Combat;
using FiveMinuteHero.Enemies;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 쿨타임마다 사거리 안의 가장 가까운 적을 맞히고, 그 적에서 가까운 적으로 여러 번 튄다. 맞은 적마다 한 번 피해를 준다.
    // 주입과 주입 확인은 Weapon이, 쿨타임 흐름은 CooldownWeapon이 맡는다. 여기는 "한 번 번개 치기"만 정한다.
    public class ChainLightningWeapon : CooldownWeapon
    {
        // 프리팹의 자식에 있는 선 이펙트. 공격마다 만들지 않고 이것 하나를 다시 쓴다.
        [SerializeField] private ChainLightningEffect _effect;

        // 이번 공격에 맞은 적. 맞은 순서대로 들어 있어 피해 순서와 선을 잇는 순서로 쓴다.
        // 공격마다 new 하지 않고 비우고 다시 쓴다.
        // 제외 검사는 Contains(선형 탐색)다. 튀는 횟수 강화가 최대 3레벨이라 많아야 7칸이어서 해시보다 싸다.
        private readonly List<Enemy> _hitTargets = new List<Enemy>();

        // 런타임 스탯은 어떤 무기인지 모르므로, 이 무기가 쓰는 스탯을 여기서 이름 붙여 꺼낸다.
        // 쿨다운은 CooldownWeapon이 직접 읽으므로 여기에 없다.
        private float Range => Stats.Get(WeaponStatType.Range);
        private float ChainRange => Stats.Get(WeaponStatType.ChainRange);
        // 강화로 소수가 생길 수 있어서 가장 가까운 정수로 맞춘다(오브 개수와 같은 방식).
        private int ChainCount => Mathf.RoundToInt(Stats.Get(WeaponStatType.ChainCount));

        // 찾기와 피해를 두 단계로 나눈다. 레지스트리(AliveEnemies)를 읽는 동안에는 목록이 바뀌지 않게 하려는 것이다.
        // 적은 죽는 즉시 레지스트리 목록에서 빠지므로(List.Remove), 순회 도중 피해를 주면 순회 중인 목록이 바뀐다.
        // #22 폭발이 OverlapCircleAll이 돌려준 별도 배열을 순회한 것과 같은 원리다. 여기서는 재사용 리스트(_hitTargets)가 그 별도 배열이다.
        // 사거리 안에 적이 없으면 아무 효과 없이 false. CooldownWeapon이 쿨타임이 찬 채로 기다린다.
        protected override bool TryAttack()
        {
            _hitTargets.Clear();

            // 1) 찾기: 첫 목표는 무기 위치에서 사거리 안의 가장 가까운 적.
            // 이 무기는 플레이어의 WeaponMount(몸 높이) 아래에 생성되므로 자기 위치가 곧 사거리의 중심이다.
            Enemy current = FindNearest(transform.position, Range);
            if (current == null)
            {
                return false;
            }

            _hitTargets.Add(current);

            // 직전에 맞은 적에서 튀는 거리 안의 가장 가까운 적으로 튄다. 없으면 남은 횟수가 있어도 거기서 멈춘다.
            int chainCount = ChainCount;
            for (int i = 0; i < chainCount; i++)
            {
                current = FindNearest(current.AimPoint, ChainRange);
                if (current == null)
                {
                    break;
                }

                _hitTargets.Add(current);
            }

            // 2) 선: 피해 전에 위치를 복사한다. 피해 뒤에는 한 방에 죽은 적이 이번 프레임 끝에 풀로 반납되므로, 그 전에 모든 자리를 잡아 둔다.
            _effect.Show(transform.position, _hitTargets);

            // 3) 피해: 찾기가 끝났으므로 여기서 적이 죽어 레지스트리 목록이 바뀌어도 상관없다.
            // 피해는 _hitTargets를 순회하며 주고, 이 리스트는 적이 죽어도 바뀌지 않는다.
            int damage = Stats.Damage;
            for (int i = 0; i < _hitTargets.Count; i++)
            {
                // 적 프리팹은 체력(IDamageable)이 Enemy와 같은 오브젝트에 있다. TryGetComponent는 할당이 없다.
                if (_hitTargets[i].TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage);
                }
            }

            return true;
        }

        // from에서 maxDistance 안의 가장 가까운 적. 이번 공격에 이미 맞은 적은 건너뛴다. 없으면 null.
        // perf-baseline 전이라 살아 있는 적 전체를 단순 순회한다. 한 번 공격에 (튀는 횟수 + 1)번 불리므로 비용은 그 배수로 늘어난다.
        private Enemy FindNearest(Vector2 from, float maxDistance)
        {
            // 제곱 거리로 비교해서 매번 제곱근을 계산하지 않는다.
            float nearestSqrDistance = maxDistance * maxDistance;
            Enemy nearest = null;

            IReadOnlyList<Enemy> enemies = Registry.AliveEnemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                float sqrDistance = (enemies[i].AimPoint - from).sqrMagnitude;
                // 거리 비교가 Contains보다 싸므로 먼저 거른다. 가까운 후보일 때만 맞은 적인지 확인한다.
                if (sqrDistance <= nearestSqrDistance && !_hitTargets.Contains(enemies[i]))
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = enemies[i];
                }
            }

            return nearest;
        }
    }
}
