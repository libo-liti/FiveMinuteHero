namespace FiveMinuteHero.Flow
{
    // 게임을 멈추고 싶은 쪽(레벨업 창, 나중에 일시정지 메뉴·부활 광고)이 쓰는 정지 요청 창구.
    // 요청한 쪽은 반드시 같은 횟수만큼 해제한다. 요청이 겹치면 마지막 해제에서만 재개된다.
    public interface IPauseService
    {
        void RequestPause();
        void ReleasePause();
    }
}
