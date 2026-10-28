using System.Collections.Generic;
using UnityEngine;

namespace FiveMinuteHero.Benchmark
{
    // 벤치마크 측정 조건. 조건을 코드가 아닌 에셋에 두어, 짧은 확인용과 실제 측정용을 에셋만 바꿔 쓴다.
    [CreateAssetMenu(fileName = "BenchmarkSettings", menuName = "FiveMinuteHero/Benchmark Settings")]
    public class BenchmarkSettings : ScriptableObject
    {
        [SerializeField] private int _seed = 12345; // 스폰 위치·종류 난수를 매 측정 같은 순서로 만든다
        [SerializeField] private int[] _enemyCounts = { 100, 300, 500, 1000 }; // 메뉴 버튼 순서와 같다
        [SerializeField, Min(0f)] private float _warmupSeconds = 10f;  // 적이 채워지고 안정될 때까지 기다리는 시간(초)
        [SerializeField, Min(1f)] private float _measureSeconds = 60f; // 실제로 기록하는 시간(초)
        [SerializeField, Min(1)] private int _maxSpawnPerFrame = 20;   // 1000마리면 약 50프레임에 걸쳐 채워진다
        [SerializeField, Min(30)] private int _targetFrameRate = 60;   // Android 기본값 30을 덮어써서 조건을 고정한다

        public int Seed => _seed;
        public IReadOnlyList<int> EnemyCounts => _enemyCounts;
        public float WarmupSeconds => _warmupSeconds;
        public float MeasureSeconds => _measureSeconds;
        public int MaxSpawnPerFrame => _maxSpawnPerFrame;
        public int TargetFrameRate => _targetFrameRate;
    }
}
