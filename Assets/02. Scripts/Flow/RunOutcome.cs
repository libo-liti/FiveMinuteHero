namespace FiveMinuteHero.Flow
{
    // 한 판이 어떻게 끝났는지. bool 두 개(클리어/사망)로 두면 둘 다 true인 상태가 생길 수 있어서 하나의 값으로 둔다.
    public enum RunOutcome
    {
        Cleared, // 제한 시간을 버텼다
        Failed   // 그 전에 죽었다
    }
}
