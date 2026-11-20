namespace FiveMinuteHero.Pickups
{
    // 바닥의 젬을 전부 플레이어에게 끌어오는 창구(자석 아이템이 쓴다).
    // GemDropper를 통째로 넘기지 않고 이 일만 열어서, 받는 쪽이 젬 드롭·개수 같은 다른 기능을 부를 수 없게 한다.
    public interface IGemAttractor
    {
        void AttractAllGems();
    }
}
