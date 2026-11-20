using UnityEngine;

namespace FiveMinuteHero.Items
{
    // 먹는 순간 바닥에 있는 젬을 전부 플레이어에게 끌어온다.
    [CreateAssetMenu(fileName = "MagnetItem", menuName = "FiveMinuteHero/Items/Magnet")]
    public class MagnetItem : ItemData
    {
        public override void Apply(ItemContext context)
        {
            context.Gems.AttractAllGems();
        }
    }
}
