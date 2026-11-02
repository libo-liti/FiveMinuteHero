using System.Collections.Generic;
using FiveMinuteHero.Player;
using FiveMinuteHero.Weapons;

namespace FiveMinuteHero.Upgrades
{
    // 강화가 바꿀 대상(런타임 스탯)을 한곳에 모아 Apply에 넘긴다.
    // 대상이 늘면 여기에만 추가하면 되고, UpgradeData.Apply 시그니처는 그대로 둘 수 있다. Bootstrap이 만든다.
    public class UpgradeContext
    {
        // WeaponData SO 에셋을 키로 쓴다. 무기마다 에셋이 하나뿐이라 참조 비교로 충분하다.
        private readonly Dictionary<WeaponData, WeaponRuntimeStats> _weapons = new Dictionary<WeaponData, WeaponRuntimeStats>();

        public UpgradeContext(PlayerRuntimeStats player)
        {
            Player = player;
        }

        public PlayerRuntimeStats Player { get; }

        // Bootstrap이 보유 무기마다 호출한다. 같은 무기를 두 번 넣으면 Add가 예외를 던져 실수를 바로 알 수 있다.
        public void RegisterWeapon(WeaponData data, WeaponRuntimeStats stats)
        {
            _weapons.Add(data, stats);
        }

        // 아직 가지지 않은 무기면 null. WeaponStatUpgrade.CanApply가 이걸로 후보에서 거른다.
        public WeaponRuntimeStats GetWeapon(WeaponData data)
        {
            return _weapons.TryGetValue(data, out WeaponRuntimeStats stats) ? stats : null;
        }
    }
}
