namespace FiveMinuteHero.Stats
{
    // 보너스를 더하는 방식. Flat은 기본값에 그대로 더하고, Percent는 0.1 = +10%(StatValue.AddPercent와 같은 규칙).
    // 런타임 스탯이 이 enum을 받으므로 Upgrades가 아니라 Stats에 둔다(Upgrades에 두면 의존 방향이 거꾸로 된다).
    // 에셋에는 숫자로 저장되므로 새 항목은 항상 끝에 추가한다.
    public enum StatModifierType { Flat, Percent }
}
