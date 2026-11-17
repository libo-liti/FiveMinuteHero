using System;

namespace FiveMinuteHero.Flow
{
    // 판이 끝났는지 읽고, 끝나는 순간을 듣는 창구(예: 레벨업 창, 결과 화면).
    // 읽기·구독만 열어서, 이 인터페이스를 받은 쪽은 판을 끝내거나 멈출 수 없다.
    public interface IRunState
    {
        bool IsRunOver { get; }
        // 판이 끝나는 순간 딱 한 번 알린다. 이미 정지가 걸린 뒤에 불린다.
        event Action<RunOutcome> RunEnded;
    }
}
