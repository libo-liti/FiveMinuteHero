using System.Globalization;

namespace Jonghyun.Core.Perf
{
    // 한 번의 측정 구간 결과. 값만 담고, 측정이 끝난 뒤에 만들어지므로 문자열 변환은 할당해도 된다.
    public readonly struct PerfReport
    {
        private const double BytesPerMegabyte = 1024d * 1024d;

        public readonly float Duration;          // 초 (unscaled)
        public readonly int FrameCount;
        public readonly float AverageFps;        // 전체 프레임 수 / 전체 시간
        public readonly float OnePercentLowFps;  // 가장 느린 1% 프레임 시간의 평균을 FPS로 바꾼 값
        // Release에서는 프레임당 할당 카운터를 쓸 수 없어서, GC가 없던 프레임의 힙 증가량 평균으로 추정한다.
        public readonly float EstimatedGcAllocPerFrameBytes;
        public readonly int GcCount;             // 구간 동안 일어난 GC 횟수
        public readonly long TotalUsedMemoryBytes; // 구간이 끝난 시점의 값
        public readonly long GcUsedMemoryBytes;    // 구간이 끝난 시점의 값

        public PerfReport(float duration, int frameCount, float averageFps, float onePercentLowFps,
            float estimatedGcAllocPerFrameBytes, int gcCount, long totalUsedMemoryBytes, long gcUsedMemoryBytes)
        {
            Duration = duration;
            FrameCount = frameCount;
            AverageFps = averageFps;
            OnePercentLowFps = onePercentLowFps;
            EstimatedGcAllocPerFrameBytes = estimatedGcAllocPerFrameBytes;
            GcCount = gcCount;
            TotalUsedMemoryBytes = totalUsedMemoryBytes;
            GcUsedMemoryBytes = gcUsedMemoryBytes;
        }

        // 로그 한 줄. label은 호출하는 쪽이 붙이는 조건 설명이다(예: 측정 조건 이름).
        public string ToLogLine(string label)
        {
            // 기기 언어에 따라 소수점이 쉼표로 바뀌지 않게 InvariantCulture로 고정한다.
            return string.Format(CultureInfo.InvariantCulture,
                "[Perf] {0} duration={1:0.0}s frames={2} avgFps={3:0.0} low1%={4:0.0} gcAllocPerFrame(est)={5:0}B gcCount={6} totalMem={7:0.0}MB gcMem={8:0.0}MB",
                label, Duration, FrameCount, AverageFps, OnePercentLowFps,
                EstimatedGcAllocPerFrameBytes, GcCount,
                TotalUsedMemoryBytes / BytesPerMegabyte, GcUsedMemoryBytes / BytesPerMegabyte);
        }

        // 화면 표시용 여러 줄.
        public string ToDisplayText(string label)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0}\nAvg FPS {1:0.0}\n1% Low {2:0.0}\nGC alloc/frame (est) {3:0} B\nGC count {4}\nTotal mem {5:0.0} MB\nGC mem {6:0.0} MB\n{7:0.0}s / {8} frames",
                label, AverageFps, OnePercentLowFps, EstimatedGcAllocPerFrameBytes, GcCount,
                TotalUsedMemoryBytes / BytesPerMegabyte, GcUsedMemoryBytes / BytesPerMegabyte,
                Duration, FrameCount);
        }
    }
}
