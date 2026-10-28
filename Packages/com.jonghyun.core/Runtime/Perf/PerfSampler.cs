using System;
using Unity.Profiling;

namespace Jonghyun.Core.Perf
{
    // 프레임 시간·GC·메모리를 모은다. 호출하는 쪽이 매 프레임 Tick을 부르고, Begin~End 구간을 PerfReport로 돌려준다.
    // Tick은 할당하지 않는다. 측정기 자신이 할당하면 GC 지표가 오염되기 때문이다.
    public sealed class PerfSampler : IDisposable
    {
        private const float LiveWindowSeconds = 0.5f; // 오버레이용 실시간 FPS를 평균 내는 구간
        private const float OnePercent = 0.01f;

        // 프레임 시간을 미리 할당한 배열에 쌓는다. 용량을 넘긴 프레임은 평균에는 들어가고 1% Low 계산에서만 빠진다.
        private readonly float[] _frameTimes;
        // Release 빌드에서도 값이 나오는 메모리 카운터만 쓴다.
        private ProfilerRecorder _totalUsedRecorder;
        private ProfilerRecorder _gcUsedRecorder;

        private bool _isRunning;
        private float _elapsed;
        private int _frameCount;
        private int _startGcCount;
        private int _lastGcCount;
        private long _lastGcUsed;
        private long _allocBytesSum;
        private int _allocFrameCount;

        private float _liveTimer;
        private int _liveFrames;
        private float _liveFps;

        public PerfSampler(int frameCapacity)
        {
            _frameTimes = new float[frameCapacity];
            _totalUsedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
            _gcUsedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
        }

        public bool IsRunning => _isRunning;
        public float Elapsed => _elapsed;
        public int FrameCount => _frameCount;
        public float LiveFps => _liveFps;
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
        }

        // 측정 여부와 관계없이 매 프레임 부른다. 실시간 값은 항상 갱신하고, 구간 기록은 Begin~End 사이에만 한다.
        public void Tick(float unscaledDeltaTime)
        {
            UpdateLive(unscaledDeltaTime);

            if (!_isRunning)
            {
                return;
            }

            if (_frameCount < _frameTimes.Length)
            {
                _frameTimes[_frameCount] = unscaledDeltaTime;
            }
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

            return new PerfReport(_elapsed, _frameCount, averageFps, CalculateOnePercentLow(), estimatedAlloc,
                gcCount, ReadTotalUsed(), ReadGcUsed());
        }

        public void Dispose()
        {
            _totalUsedRecorder.Dispose();
            _gcUsedRecorder.Dispose();
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

        // 구간이 끝난 뒤 한 번만 정렬한다. 사본을 만들어 할당하지만 측정 구간 밖이라 결과에 영향이 없다.
        private float CalculateOnePercentLow()
        {
            int stored = Math.Min(_frameCount, _frameTimes.Length);
            if (stored == 0)
            {
                return 0f;
            }

            float[] sorted = new float[stored];
            Array.Copy(_frameTimes, sorted, stored);
            Array.Sort(sorted);

            // 가장 느린(프레임 시간이 긴) 쪽 1%의 평균. 프레임이 100개 미만이어도 최소 1개는 본다.
            int worstCount = Math.Max(1, (int)(stored * OnePercent));
            double sum = 0d;
            for (int i = stored - worstCount; i < stored; i++)
            {
                sum += sorted[i];
            }

            double averageWorst = sum / worstCount;
            return averageWorst > 0d ? (float)(1d / averageWorst) : 0f;
        }

        // LastValue는 프레임이 끝나야 채워져서 시작 직후엔 0이다. 지금 값을 바로 읽는 CurrentValue를 쓴다.
        // 카운터를 못 쓰는 환경이면 .NET API로 대신 읽는다.
        private long ReadGcUsed()
        {
            return _gcUsedRecorder.Valid ? _gcUsedRecorder.CurrentValue : GC.GetTotalMemory(false);
        }

        private long ReadTotalUsed()
        {
            return _totalUsedRecorder.Valid ? _totalUsedRecorder.CurrentValue : 0L;
        }
    }
}
