using System.Globalization;

namespace Jonghyun.Core.Perf
{
    // 한 번의 측정 구간 결과. 값만 담고, 측정이 끝난 뒤에 만들어지므로 문자열 변환은 할당해도 된다.
    // 못 잰 값은 0과 구분해 "미지원"으로 표시한다(0ms나 0 Batches를 정상값으로 오해하지 않게).
    public readonly struct PerfReport
    {
        private const double BytesPerMegabyte = 1024d * 1024d;
        private const string UnavailableText = "미지원";

        public readonly float Duration;          // 초 (unscaled)
        public readonly int FrameCount;
        public readonly float AverageFps;        // 전체 프레임 수 / 전체 시간
        public readonly float OnePercentLowFps;  // 가장 느린 1% 프레임 시간의 평균을 FPS로 바꾼 값
        // Release에서는 프레임당 할당 카운터를 쓸 수 없어서, GC가 없던 프레임의 힙 증가량 평균으로 추정한다.
        public readonly float EstimatedGcAllocPerFrameBytes;
        public readonly int GcCount;             // 구간 동안 일어난 GC 횟수
        public readonly bool HasTotalUsedMemory; // false면 TotalUsedMemoryBytes는 못 잰 값이다
        public readonly long TotalUsedMemoryBytes; // 구간이 끝난 시점의 값
        public readonly long GcUsedMemoryBytes;    // 구간이 끝난 시점의 값

        // vsync 대기를 뺀 실제 작업 시간(ms). 60Hz 기기에서도 Before/After 차이가 보인다.
        public readonly PerfTimeStat CpuMainThread;
        public readonly PerfTimeStat CpuRenderThread;
        public readonly PerfTimeStat Gpu;

        public readonly bool HasRenderStats;      // false면 아래 두 값은 못 잰 값이다
        public readonly float AverageBatches;     // 프레임당 평균
        public readonly float AverageSetPassCalls; // 프레임당 평균

        public PerfReport(float duration, int frameCount, float averageFps, float onePercentLowFps,
            float estimatedGcAllocPerFrameBytes, int gcCount,
            bool hasTotalUsedMemory, long totalUsedMemoryBytes, long gcUsedMemoryBytes,
            PerfTimeStat cpuMainThread, PerfTimeStat cpuRenderThread, PerfTimeStat gpu,
            bool hasRenderStats, float averageBatches, float averageSetPassCalls)
        {
            Duration = duration;
            FrameCount = frameCount;
            AverageFps = averageFps;
            OnePercentLowFps = onePercentLowFps;
            EstimatedGcAllocPerFrameBytes = estimatedGcAllocPerFrameBytes;
            GcCount = gcCount;
            HasTotalUsedMemory = hasTotalUsedMemory;
            TotalUsedMemoryBytes = totalUsedMemoryBytes;
            GcUsedMemoryBytes = gcUsedMemoryBytes;
            CpuMainThread = cpuMainThread;
            CpuRenderThread = cpuRenderThread;
            Gpu = gpu;
            HasRenderStats = hasRenderStats;
            AverageBatches = averageBatches;
            AverageSetPassCalls = averageSetPassCalls;
        }

        // 로그 한 줄. label은 호출하는 쪽이 붙이는 조건 설명이다(예: 측정 조건 이름).
        // 시간 지표는 "평균/최악1%(샘플 수)" 형식이다. 샘플 수가 frames보다 적으면 못 잰 프레임이 있었다는 뜻이다.
        public string ToLogLine(string label)
        {
            // 기기 언어에 따라 소수점이 쉼표로 바뀌지 않게 InvariantCulture로 고정한다.
            return string.Format(CultureInfo.InvariantCulture,
                "[Perf] {0} duration={1:0.0}s frames={2} avgFps={3:0.0} low1%={4:0.0} " +
                "cpuMainMs={5} cpuRenderMs={6} gpuMs={7} batches={8} setPass={9} " +
                "gcAllocPerFrame(est)={10:0}B gcCount={11} totalMem={12} gcMem={13:0.0}MB",
                label, Duration, FrameCount, AverageFps, OnePercentLowFps,
                FormatTimeForLog(CpuMainThread), FormatTimeForLog(CpuRenderThread), FormatTimeForLog(Gpu),
                FormatCount(HasRenderStats, AverageBatches), FormatCount(HasRenderStats, AverageSetPassCalls),
                EstimatedGcAllocPerFrameBytes, GcCount,
                FormatMegabytes(HasTotalUsedMemory, TotalUsedMemoryBytes), GcUsedMemoryBytes / BytesPerMegabyte);
        }

        // 화면 표시용 여러 줄.
        public string ToDisplayText(string label)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0}\nAvg FPS {1:0.0}\n1% Low {2:0.0}\n" +
                "CPU main {3}\nCPU render {4}\nGPU {5}\n" +
                "Batches {6}\nSetPass {7}\n" +
                "GC alloc/frame (est) {8:0} B\nGC count {9}\nTotal mem {10}\nGC mem {11:0.0} MB\n{12:0.0}s / {13} frames",
                label, AverageFps, OnePercentLowFps,
                FormatTimeForDisplay(CpuMainThread), FormatTimeForDisplay(CpuRenderThread), FormatTimeForDisplay(Gpu),
                FormatCount(HasRenderStats, AverageBatches), FormatCount(HasRenderStats, AverageSetPassCalls),
                EstimatedGcAllocPerFrameBytes, GcCount,
                FormatMegabytes(HasTotalUsedMemory, TotalUsedMemoryBytes), GcUsedMemoryBytes / BytesPerMegabyte,
                Duration, FrameCount);
        }

        private static string FormatTimeForLog(PerfTimeStat stat)
        {
            if (!stat.IsAvailable)
            {
                return UnavailableText;
            }

            return string.Format(CultureInfo.InvariantCulture, "{0:0.00}/{1:0.00}(n={2})",
                stat.AverageMs, stat.WorstOnePercentMs, stat.SampleCount);
        }

        private static string FormatTimeForDisplay(PerfTimeStat stat)
        {
            if (!stat.IsAvailable)
            {
                return UnavailableText;
            }

            return string.Format(CultureInfo.InvariantCulture, "{0:0.00} ms (1% {1:0.00})",
                stat.AverageMs, stat.WorstOnePercentMs);
        }

        private static string FormatCount(bool isAvailable, float value)
        {
            return isAvailable ? value.ToString("0.0", CultureInfo.InvariantCulture) : UnavailableText;
        }

        private static string FormatMegabytes(bool isAvailable, long bytes)
        {
            return isAvailable
                ? string.Format(CultureInfo.InvariantCulture, "{0:0.0}MB", bytes / BytesPerMegabyte)
                : UnavailableText;
        }
    }
}
