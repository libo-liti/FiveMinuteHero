using FiveMinuteHero.Player;
using FiveMinuteHero.Weapons;

namespace FiveMinuteHero.Upgrades
{
    // 강화가 바꿀 대상(런타임 스탯)을 한곳에 모아 Apply에 넘긴다.
    // 대상이 늘면 여기에만 추가하면 되고, UpgradeData.Apply 시그니처는 그대로 둘 수 있다. Bootstrap이 만든다.
    public class UpgradeContext
    {
        public UpgradeContext(PlayerRuntimeStats player, IWeaponInventory weapons)
        {
            Player = player;
            Weapons = weapons;
        }

        public PlayerRuntimeStats Player { get; }
        // 무기 목록은 인벤토리가 들고, 여기서는 창구만 건넨다. 판 중간에 얻은 무기도 인벤토리에 들어가면 바로 보인다.
        public IWeaponInventory Weapons { get; }
    }
}
