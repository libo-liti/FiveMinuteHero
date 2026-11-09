namespace Jonghyun.Core.Perf
{
    // 시간 지표 하나(예: CPU 메인 스레드 시간)의 구간 결과.
    // "못 잰 값"을 0ms와 구분하려고 IsAvailable을 따로 둔다. default 값은 IsAvailable이 false라 그대로 "미지원"이 된다.
    public readonly struct PerfTimeStat
    {
        public static readonly PerfTimeStat Unavailable = default;

        public readonly bool IsAvailable;
        public readonly float AverageMs;         // 0보다 큰 샘플만의 평균
        public readonly float WorstOnePercentMs; // 가장 느린 1% 샘플의 평균 (FPS 1% Low와 같은 계산)
        public readonly int SampleCount;         // 평균에 들어간 프레임 수. 측정 구간 프레임 수보다 적으면 그만큼 못 잰 프레임이 있었다는 뜻이다.

        public PerfTimeStat(float averageMs, float worstOnePercentMs, int sampleCount)
        {
            IsAvailable = true;
            AverageMs = averageMs;
            WorstOnePercentMs = worstOnePercentMs;
            SampleCount = sampleCount;
        }
    }
}
