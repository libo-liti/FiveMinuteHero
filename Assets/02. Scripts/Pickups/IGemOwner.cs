namespace FiveMinuteHero.Pickups
{
    // 젬이 자기를 만든 쪽(목록을 든 쪽)에 "나를 목록에서 빼 줘"라고 알리는 창구.
    // 젬에게 GemDropper를 통째로 주지 않고 이 일만 열어서, 젬이 자석(AttractAllGems)이나 개수 조회를 부를 수 없게 한다.
    public interface IGemOwner
    {
        void Unregister(ExpGem gem);
    }
}
