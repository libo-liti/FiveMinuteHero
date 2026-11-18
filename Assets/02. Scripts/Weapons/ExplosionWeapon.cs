using System.Collections.Generic;
using FiveMinuteHero.Combat;
using FiveMinuteHero.Enemies;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 쿨타임마다 사거리 안의 적 하나를 무작위로 골라 그 자리에서 폭발한다.
    // 주입과 주입 확인은 Weapon이, 쿨타임 흐름은 CooldownWeapon이 맡는다. 여기는 "한 번 폭발하기"만 정한다.
    // 가장 가까운 적은 마법탄이 맡으므로, 이 무기는 무작위로 골라 뒤쪽 적까지 닿게 한다.
    public class ExplosionWeapon : CooldownWeapon
    {
        // 수치가 아니라 이 무기의 외형이라서 런타임 스탯이 아니라 무기 쪽이 들고 있다.
        [SerializeField] private ExplosionEffect _effectPrefab;

        // 폭발이 맞힐 레이어(Enemy). 없으면 원 안의 플레이어·투사체 콜라이더까지 결과에 들어온다.
        [SerializeField] private LayerMask _enemyLayers;

        // 폭발마다 새 리스트를 만들지 않도록 비우고 다시 쓴다.
        private readonly List<Enemy> _candidates = new List<Enemy>();

        // 런타임 스탯은 어떤 무기인지 모르므로, 이 무기가 쓰는 스탯을 여기서 이름 붙여 꺼낸다.
        // 쿨다운은 CooldownWeapon이 직접 읽으므로 여기에 없다.
        private float Range => Stats.Get(WeaponStatType.Range);
        private float Radius => Stats.Get(WeaponStatType.ExplosionRadius);

        // 사거리 안에 적이 없으면 아무것도 하지 않고 false. CooldownWeapon이 쿨타임이 찬 채로 기다린다.
        // 목표는 적이지만 폭발에 필요한 것은 위치뿐이라, 적의 위치만 넘긴다. 부모는 이 차이를 모른다.
        protected override bool TryAttack()
        {
            Enemy target = PickRandomTarget();
            if (target == null)
            {
                return false;
            }

            Explode(target.AimPoint);
            return true;
        }

        // perf-baseline 전이라 살아 있는 적 전체를 단순 순회한다(O(n)). 사거리 안에 적이 없으면 null.
        private Enemy PickRandomTarget()
        {
            // 이 무기는 플레이어의 WeaponMount(몸 높이) 아래에 생성되므로 자기 위치가 곧 사거리의 중심이다.
            Vector2 origin = transform.position;
            // 제곱 거리로 비교해서 매번 제곱근을 계산하지 않는다.
            float sqrRange = Range * Range;

            _candidates.Clear();
            IReadOnlyList<Enemy> enemies = Registry.AliveEnemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                if ((enemies[i].AimPoint - origin).sqrMagnitude <= sqrRange)
                {
                    _candidates.Add(enemies[i]);
                }
            }

            if (_candidates.Count == 0)
            {
                return null;
            }

            // Random.Range(int, int)는 끝 값을 포함하지 않으므로 Count를 그대로 넘긴다.
            return _candidates[Random.Range(0, _candidates.Count)];
        }

        // center에서 반경 안의 적 전체에게 한 번씩 피해를 준다.
        private void Explode(Vector2 center)
        {
            // 결과는 질의 때 만든 별도 배열이다. 피해로 적이 죽어 EnemyRegistry 목록이 바뀌어도 이 순회에는 영향이 없다.
            // perf-baseline 전이라 호출마다 새 배열을 받는다. 적 프리팹은 콜라이더가 1개라서 같은 적이 두 번 들어오지 않는다.
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, Radius, _enemyLayers);

            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(Stats.Damage);
                }
            }

            // 플레이어를 따라 움직이지 않도록 부모 없이 만든다.
            ExplosionEffect effect = Instantiate(_effectPrefab, center, Quaternion.identity);
            effect.Play(Radius);
        }
    }
}
