using System;
using Unity.Profiling;
using UnityEngine;

namespace Jonghyun.Core.Perf
{
    // 프레임 시간·GC·메모리를 모은다. 호출하는 쪽이 매 프레임 Tick을 부르고, Begin~End 구간을 PerfReport로 돌려준다.
    // Tick은 할당하지 않는다. 측정기 자신이 할당하면 GC 지표가 오염되기 때문이다.
    public sealed class PerfSampler : IDisposable
    {
        private const float LiveWindowSeconds = 0.5f; // 오버레이용 실시간 FPS를 평균 내는 구간
        private const float OnePercent = 0.01f;
        private const uint LatestTimingCount = 1;     // 매 프레임 가장 최근 1프레임 결과만 읽는다

        // 프레임 시간을 미리 할당한 배열에 쌓는다. 용량을 넘긴 프레임은 평균에는 들어가고 1% Low 계산에서만 빠진다.
        private readonly float[] _frameTimes;
        // vsync 대기를 뺀 "실제로 일한 시간"(ms). 60Hz 기기에서 FPS가 60에 막혀도 차이가 보인다.
        private readonly FrameTimeBuffer _cpuMainTimes;
        private readonly FrameTimeBuffer _cpuRenderTimes;
        private readonly FrameTimeBuffer _gpuTimes;
        private readonly FrameTiming[] _timingBuffer = new FrameTiming[LatestTimingCount]; // GetLatestTimings가 채우는 재사용 배열
        private readonly bool _isFrameTimingEnabled; // Release 빌드에서 Frame Timing Stats가 꺼져 있으면 false
        // Release 빌드에서도 값이 나오는 메모리 카운터만 쓴다.
        private ProfilerRecorder _totalUsedRecorder;
        private ProfilerRecorder _gcUsedRecorder;
        // 렌더링 카운터. Unity 문서상 Release 빌드에서도 읽을 수 있다.
        private ProfilerRecorder _batchesRecorder;
        private ProfilerRecorder _setPassRecorder;

        private bool _isRunning;
        private float _elapsed;
        private int _frameCount;
        private int _startGcCount;
        private int _lastGcCount;
        private long _lastGcUsed;
        private long _allocBytesSum;
        private int _allocFrameCount;
        private long _batchesSum;
        private long _setPassSum;

        private float _liveTimer;
        private int _liveFrames;
        private float _liveFps;
        private float _liveCpuMainMs;
        private float _liveCpuRenderMs;
        private float _liveGpuMs;

        public PerfSampler(int frameCapacity)
        {
            _frameTimes = new float[frameCapacity];
            _cpuMainTimes = new FrameTimeBuffer(frameCapacity);
            _cpuRenderTimes = new FrameTimeBuffer(frameCapacity);
            _gpuTimes = new FrameTimeBuffer(frameCapacity);
            _totalUsedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
            _gcUsedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
            _batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            _setPassRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");

            // 빌드 설정이라 실행 중에 바뀌지 않으므로 한 번만 확인한다.
            _isFrameTimingEnabled = FrameTimingManager.IsFeatureEnabled();
            if (!_isFrameTimingEnabled)
            {
                Debug.LogWarning("PerfSampler: FrameTimingManager가 꺼져 있어 CPU/GPU 시간을 잴 수 없습니다. Player Settings > Frame Timing Stats를 켜세요.");
            }
        }

        public bool IsRunning => _isRunning;
        public float Elapsed => _elapsed;
        public int FrameCount => _frameCount;
        public float LiveFps => _liveFps;
        // 실시간 시간 값은 가장 최근 프레임 값이다. 0이면 아직 결과가 안 왔거나 이 기기에서 못 재는 값이다.
        public bool IsFrameTimingEnabled => _isFrameTimingEnabled;
        public float LiveCpuMainMs => _liveCpuMainMs;
        public float LiveCpuRenderMs => _liveCpuRenderMs;
        public float LiveGpuMs => _liveGpuMs;
        // recorder가 Valid가 아니면 그 카운터는 이 환경에 없다. 0 대신 "미지원"으로 보여 주는 데 쓴다.
        public bool HasRenderStats => _batchesRecorder.Valid && _setPassRecorder.Valid;
        public bool HasTotalUsedMemory => _totalUsedRecorder.Valid;
        // LastValue는 직전에 끝난 프레임의 값이다(이번 프레임은 아직 그리는 중이라).
        public long LiveBatches => ReadLastValue(_batchesRecorder);
        public long LiveSetPassCalls => ReadLastValue(_setPassRecorder);
        public long LiveTotalUsedMemory => ReadTotalUsed();
        public long LiveGcUsedMemory => ReadGcUsed();
        public int LiveGcCount => GC.CollectionCount(0);

        public void Begin()
        {
            _isRunning = true;
            _elapsed = 0f;
            _frameCount = 0;
            _allocBytesSum = 0;
            _allocFrameCount = 0;
            _startGcCount = GC.CollectionCount(0);
            _lastGcCount = _startGcCount;
            _lastGcUsed = ReadGcUsed();
            _cpuMainTimes.Reset();
            _cpuRenderTimes.Reset();
            _gpuTimes.Reset();
            _batchesSum = 0;
            _setPassSum = 0;
        }

        // 측정 여부와 관계없이 매 프레임 부른다. 실시간 값은 항상 갱신하고, 구간 기록은 Begin~End 사이에만 한다.
        public void Tick(float unscaledDeltaTime)
        {
            UpdateLive(unscaledDeltaTime);
            ReadFrameTiming();

            if (!_isRunning)
            {
                return;
            }

            if (_frameCount < _frameTimes.Length)
            {
                _frameTimes[_frameCount] = unscaledDeltaTime;
            }
            _cpuMainTimes.Add(_liveCpuMainMs);
            _cpuRenderTimes.Add(_liveCpuRenderMs);
            _gpuTimes.Add(_liveGpuMs);
            _batchesSum += ReadLastValue(_batchesRecorder);
            _setPassSum += ReadLastValue(_setPassRecorder);
            _frameCount++;
            _elapsed += unscaledDeltaTime;

            // GC가 돈 프레임은 힙이 줄어 증가량을 알 수 없으므로 뺀다. 줄어든 값(음수)도 0으로 본다.
            int gcCount = GC.CollectionCount(0);
            long gcUsed = ReadGcUsed();
            if (gcCount == _lastGcCount)
            {
                long delta = gcUsed - _lastGcUsed;
                if (delta > 0)
                {
                    _allocBytesSum += delta;
                }
                _allocFrameCount++;
            }
            _lastGcCount = gcCount;
            _lastGcUsed = gcUsed;
        }

        public PerfReport End()
        {
            _isRunning = false;

            float averageFps = _elapsed > 0f ? _frameCount / _elapsed : 0f;
            float estimatedAlloc = _allocFrameCount > 0 ? (float)_allocBytesSum / _allocFrameCount : 0f;
            int gcCount = GC.CollectionCount(0) - _startGcCount;

            // 가장 느린 1% 프레임 시간(초)의 평균을 FPS로 바꾼다.
            float worstFrameSeconds = AverageOfWorstOnePercent(_frameTimes, _frameCount);
            float onePercentLowFps = worstFrameSeconds > 0f ? 1f / worstFrameSeconds : 0f;

            float averageBatches = _frameCount > 0 ? (float)_batchesSum / _frameCount : 0f;
            float averageSetPass = _frameCount > 0 ? (float)_setPassSum / _frameCount : 0f;

            // 인자가 많아 이름을 붙여 넘긴다. 순서를 잘못 넣어도 컴파일 에러로 잡힌다.
            return new PerfReport(
                duration: _elapsed,
                frameCount: _frameCount,
                averageFps: averageFps,
                onePercentLowFps: onePercentLowFps,
                estimatedGcAllocPerFrameBytes: estimatedAlloc,
                gcCount: gcCount,
                hasTotalUsedMemory: HasTotalUsedMemory,
                totalUsedMemoryBytes: ReadTotalUsed(),
                gcUsedMemoryBytes: ReadGcUsed(),
                cpuMainThread: _cpuMainTimes.ToStat(),
                cpuRenderThread: _cpuRenderTimes.ToStat(),
                gpu: _gpuTimes.ToStat(),
                hasRenderStats: HasRenderStats,
                averageBatches: averageBatches,
                averageSetPassCalls: averageSetPass);
        }

        public void Dispose()
        {
            _totalUsedRecorder.Dispose();
            _gcUsedRecorder.Dispose();
            _batchesRecorder.Dispose();
            _setPassRecorder.Dispose();
        }

        private void UpdateLive(float unscaledDeltaTime)
        {
            _liveTimer += unscaledDeltaTime;
            _liveFrames++;
            if (_liveTimer < LiveWindowSeconds)
            {
                return;
            }

            _liveFps = _liveFrames / _liveTimer;
            _liveTimer = 0f;
            _liveFrames = 0;
        }

        // FrameTimingManager는 4프레임 늦게 결과를 준다. 그래서 이번 프레임에 읽는 값은 몇 프레임 전 것이지만,
        // 60초 구간에서 앞뒤 4프레임 차이는 무시한다. 재사용 배열에 채우므로 할당이 없다.
        private void ReadFrameTiming()
        {
            if (!_isFrameTimingEnabled)
            {
                return;
            }

            FrameTimingManager.CaptureFrameTimings();
            uint count = FrameTimingManager.GetLatestTimings(LatestTimingCount, _timingBuffer);
            if (count == 0)
            {
                // 시작 직후처럼 아직 결과가 없는 프레임. 0은 "못 잰 값"으로 다룬다.
                _liveCpuMainMs = 0f;
                _liveCpuRenderMs = 0f;
                _liveGpuMs = 0f;
                return;
            }

            // FrameTiming의 시간 필드는 ms 단위 double이다.
            FrameTiming timing = _timingBuffer[0];
            _liveCpuMainMs = (float)timing.cpuMainThreadFrameTime;
            _liveCpuRenderMs = (float)timing.cpuRenderThreadFrameTime;
            _liveGpuMs = (float)timing.gpuFrameTime;
        }

        // 가장 느린(값이 큰) 쪽 1%의 평균. 프레임 시간(초)과 CPU/GPU 시간(ms) 배열에 똑같이 쓴다.
        // 구간이 끝난 뒤 한 번만 정렬한다. 사본을 만들어 할당하지만 측정 구간 밖이라 결과에 영향이 없다.
        private static float AverageOfWorstOnePercent(float[] values, int count)
        {
            int stored = Math.Min(count, values.Length);
            if (stored == 0)
            {
                return 0f;
            }

            float[] sorted = new float[stored];
            Array.Copy(values, sorted, stored);
            Array.Sort(sorted);

            // 샘플이 100개 미만이어도 최소 1개는 본다.
            int worstCount = Math.Max(1, (int)(stored * OnePercent));
            double sum = 0d;
            for (int i = stored - worstCount; i < stored; i++)
            {
                sum += sorted[i];
            }

            return (float)(sum / worstCount);
        }

        // 시간 지표 하나의 구간 기록. 0 이하(못 잰 프레임)는 버려서 평균이 0 쪽으로 끌려가지 않게 한다.
        // 배열은 생성자에서 한 번만 만들고, Add는 할당하지 않는다.
        private sealed class FrameTimeBuffer
        {
            private readonly float[] _values;
            private int _count;
            private double _sum;

            public FrameTimeBuffer(int capacity)
            {
                _values = new float[capacity];
            }

            public void Reset()
            {
                _count = 0;
                _sum = 0d;
            }

            public void Add(float milliseconds)
            {
                if (milliseconds <= 0f)
                {
                    return;
                }

                // _frameTimes와 같은 규칙: 용량을 넘긴 샘플은 평균에만 들어가고 1% 계산에서 빠진다.
                if (_count < _values.Length)
                {
                    _values[_count] = milliseconds;
                }
                _count++;
                _sum += milliseconds;
            }

            // 한 번도 못 쟀으면 0ms가 아니라 "미지원"을 돌려준다.
            public PerfTimeStat ToStat()
            {
                if (_count == 0)
                {
                    return PerfTimeStat.Unavailable;
                }

                return new PerfTimeStat((float)(_sum / _count), AverageOfWorstOnePercent(_values, _count), _count);
            }
        }

        // LastValue는 프레임이 끝나야 채워져서 시작 직후엔 0이다. 지금 값을 바로 읽는 CurrentValue를 쓴다.
        // 카운터를 못 쓰는 환경이면 .NET API로 대신 읽는다.
        private long ReadGcUsed()
        {
            return _gcUsedRecorder.Valid ? _gcUsedRecorder.CurrentValue : GC.GetTotalMemory(false);
        }

        // Valid가 아니면 0을 돌려주지만, 표시하는 쪽은 HasTotalUsedMemory로 "미지원"을 구분한다.
        private long ReadTotalUsed()
        {
            return _totalUsedRecorder.Valid ? _totalUsedRecorder.CurrentValue : 0L;
        }

        // 렌더 카운터는 프레임이 끝나야 값이 정해지므로 CurrentValue가 아니라 직전 프레임의 LastValue를 쓴다.
        private static long ReadLastValue(ProfilerRecorder recorder)
        {
            return recorder.Valid ? recorder.LastValue : 0L;
        }
    }
}
