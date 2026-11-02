using FiveMinuteHero.Player;
using FiveMinuteHero.Stats;
using UnityEngine;

namespace FiveMinuteHero.Upgrades
{
    // 플레이어 스탯 하나(이동 속도, 최대 체력, 흡수 범위)를 올리는 강화.
    [CreateAssetMenu(fileName = "PlayerStatUpgrade", menuName = "FiveMinuteHero/Upgrades/Player Stat")]
    public class PlayerStatUpgrade : UpgradeData
    {
        // Percent 값(0.1)을 화면에 %로(10) 보여 줄 때 곱하는 수.
        private const float PercentDisplayScale = 100f;

        [SerializeField] private PlayerStatType _stat;
        [SerializeField] private StatModifierType _modifierType;

        public override string GetDescription(int level)
        {
            float value = GetValue(level);
            return FormatDescription(_modifierType == StatModifierType.Percent ? value * PercentDisplayScale : value);
        }

        public override void Apply(UpgradeContext context, int level)
        {
            context.Player.AddBonus(_stat, _modifierType, GetValue(level));
        }
    }
}
