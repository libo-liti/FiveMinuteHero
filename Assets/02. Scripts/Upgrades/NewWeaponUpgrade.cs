using FiveMinuteHero.Weapons;
using UnityEngine;

namespace FiveMinuteHero.Upgrades
{
    // 판 중간에 새 무기를 얻는 강화. 아직 가지지 않은 무기일 때만 후보에 나온다.
    // 무기 종류를 모른다. 에셋의 _weapon만 바꾸면 어떤 무기든 얻는 카드가 된다.
    // 에셋의 _valuesPerLevel에는 값을 1개만 넣는다. 값의 개수가 MaxLevel이라, 한 번 얻으면 후보에서 빠진다(값 자체는 쓰지 않는다).
    [CreateAssetMenu(fileName = "NewWeaponUpgrade", menuName = "FiveMinuteHero/Upgrades/New Weapon")]
    public class NewWeaponUpgrade : UpgradeData
    {
        [SerializeField] private WeaponData _weapon;

        // WeaponStatUpgrade와 정반대 조건이다. 이 카드로 무기를 얻으면 그 무기의 강화가 후보에 나오기 시작한다.
        public override bool CanApply(UpgradeContext context)
        {
            return !context.Weapons.Has(_weapon);
        }

        // 표시할 숫자가 없어서 설명 문장에 {0}을 쓰지 않는다. 값은 형식 문자열에 없으면 무시된다.
        public override string GetDescription(int level)
        {
            return FormatDescription(GetValue(level));
        }

        public override void Apply(UpgradeContext context, int level)
        {
            context.Weapons.Add(_weapon);
        }
    }
}
