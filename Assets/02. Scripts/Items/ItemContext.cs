using FiveMinuteHero.Pickups;
using FiveMinuteHero.Player;

namespace FiveMinuteHero.Items
{
    // 아이템 효과가 바꿀 대상을 한곳에 모아 Apply에 넘긴다(UpgradeContext와 같은 방식). Bootstrap이 만든다.
    // 대상이 늘면 여기에만 추가하면 되고, ItemData.Apply 시그니처는 그대로 둘 수 있다.
    // SO가 아니라 일반 클래스인 이유: 담는 값이 씬의 컴포넌트(런타임 객체)라 에셋에 저장할 수 없고, 판마다 새로 만들어야 한다.
    public class ItemContext
    {
        public ItemContext(PlayerHealth health, IGemAttractor gems)
        {
            Health = health;
            Gems = gems;
        }

        public PlayerHealth Health { get; }
        // 젬 목록 전체가 아니라 "다 끌어오기" 창구만 건넨다. 아이템이 젬을 빼거나 세는 일은 할 수 없다.
        public IGemAttractor Gems { get; }
    }
}
