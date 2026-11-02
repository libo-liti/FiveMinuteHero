using FiveMinuteHero.Stats;
using FiveMinuteHero.Weapons;
using UnityEngine;

namespace FiveMinuteHero.Upgrades
{
    // 특정 무기의 스탯 하나를 올리는 강화. 그 무기를 가지고 있을 때만 후보에 나온다.
    [CreateAssetMenu(fileName = "WeaponStatUpgrade", menuName = "FiveMinuteHero/Upgrades/Weapon Stat")]
    public class WeaponStatUpgrade : UpgradeData
    {
        // Percent 값(0.1)을 화면에 %로(10) 보여 줄 때 곱하는 수.
        private const float PercentDisplayScale = 100f;

        [SerializeField] private WeaponData _weapon;
        [SerializeField] private WeaponStatType _stat;
        [SerializeField] private StatModifierType _modifierType;

        // 무기가 늘어도 후보 뽑기 코드는 그대로 두고, 이 판단만으로 안 가진 무기의 강화를 거른다.
        public override bool CanApply(UpgradeContext context)
        {
            return context.GetWeapon(_weapon) != null;
        }

        public override string GetDescription(int level)
        {
            float value = GetValue(level);
            return FormatDescription(_modifierType == StatModifierType.Percent ? value * PercentDisplayScale : value);
        }

        public override void Apply(UpgradeContext context, int level)
        {
            WeaponRuntimeStats weaponStats = context.GetWeapon(_weapon);
            // CanApply로 이미 걸렀으므로 여기서 null이면 호출 순서가 잘못된 것이다. 조용히 넘기지 않고 알린다.
            if (weaponStats == null)
            {
                Debug.LogError("WeaponStatUpgrade: 가지지 않은 무기에 강화를 적용하려 했습니다.", this);
                return;
            }

            weaponStats.AddBonus(_stat, _modifierType, GetValue(level));
        }
    }
}
