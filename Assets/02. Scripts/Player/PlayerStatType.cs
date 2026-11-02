namespace FiveMinuteHero.Player
{
    // 강화로 바꿀 수 있는 플레이어 스탯. 무적 시간은 강화 대상이 아니라 넣지 않는다.
    // 에셋에는 숫자로 저장되므로 중간에 끼우거나 순서를 바꾸면 기존 에셋이 다른 스탯을 가리킨다 → 새 항목은 항상 끝에 추가한다.
    public enum PlayerStatType { MoveSpeed, MaxHealth, PickupRange }
}
