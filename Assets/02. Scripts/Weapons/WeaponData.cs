using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 모든 무기가 함께 갖는 기본값. 무기마다 다른 값은 하위 SO(MagicBoltData 등)가 가진다.
    // 추상 클래스는 에셋으로 만들 수 없어서 [CreateAssetMenu]는 하위 클래스에 붙인다.
    public abstract class WeaponData : ScriptableObject
    {
        // [Min]과 런타임 최솟값이 같은 상수를 쓴다. 한 곳만 고치면 둘 다 바뀐다.
        private const float MinDamage = 0f;

        [SerializeField, Min(MinDamage)] private int _damage = 5;
        // 어떻게 싸울지(행동)와 외형은 프리팹 쪽이 맡는다. 인벤토리가 이 프리팹을 만들어 Init한다.
        [SerializeField] private Weapon _weaponPrefab;

        public Weapon WeaponPrefab => _weaponPrefab;

        // 이번 판에 쓸 런타임 스탯을 새로 만든다. SO 값은 기본값으로 읽기만 하고 고치지 않는다.
        // 공통 스탯은 여기서 등록하고, 무기별 스탯은 DefineStats로 하위 클래스에 맡긴다.
        public WeaponRuntimeStats CreateRuntimeStats()
        {
            WeaponRuntimeStats stats = new WeaponRuntimeStats();
            stats.Define(WeaponStatType.Damage, _damage, MinDamage);
            DefineStats(stats);
            // 여기서 잠가 두면, 이 메서드 밖에서는 아무도 스탯을 추가할 수 없다.
            stats.Lock();
            return stats;
        }

        // 이 무기만 가진 스탯을 등록한다. 여기에 없는 스탯은 강화해도 LogError로 막힌다.
        protected abstract void DefineStats(WeaponRuntimeStats stats);
    }
}
