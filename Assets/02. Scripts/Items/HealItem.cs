using UnityEngine;

namespace FiveMinuteHero.Items
{
    // 최대 체력의 일정 비율만큼 체력을 회복한다.
    [CreateAssetMenu(fileName = "HealItem", menuName = "FiveMinuteHero/Items/Heal")]
    public class HealItem : ItemData
    {
        // 고정 수치가 아니라 비율인 이유: 최대 체력 강화를 고른 뒤에도 회복의 체감이 줄지 않는다.
        [SerializeField, Range(0f, 1f)] private float _healRatio = 0.3f;

        public override void Apply(ItemContext context)
        {
            // 올림: 최대 체력이 작아도(예: 10 × 0.05 = 0.5) 최소 1은 회복되게 한다. 최대치에서 자르는 일은 Heal이 한다.
            int amount = Mathf.CeilToInt(context.Health.MaxHealth * _healRatio);
            context.Health.Heal(amount);
        }
    }
}
