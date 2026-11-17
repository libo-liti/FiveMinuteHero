namespace FiveMinuteHero.Flow
{
    // 판을 끝내 달라고 요청하는 창구(예: 제한 시간을 다 버틴 StageRunner).
    // 요청하는 쪽이 GameSession의 다른 기능(정지, 재시작)을 모르게 하려고 이 메서드 하나만 연다.
    public interface IRunEndService
    {
        // 이미 판이 끝났으면 무시된다. 먼저 온 요청 하나만 결과가 된다.
        void EndRun(RunOutcome outcome);
    }
}
