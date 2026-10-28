using System.Globalization;
using FiveMinuteHero.Enemies;
using FiveMinuteHero.Player;
using Jonghyun.Core.Perf;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FiveMinuteHero.Benchmark
{
    // 벤치마크 씬의 진행을 맡는다: 적 수 선택 → 워밍업 → 측정 → 결과(화면 + 로그).
    // 숫자를 모으는 일은 Core.Perf의 PerfSampler가 하고, 여기서는 게임 조건(적 수, 시드, 무적)만 맞춘다.
    public class BenchmarkRunner : MonoBehaviour
    {
        private const float PausedTimeScale = 0f;
        private const float NormalTimeScale = 1f;
        // 프레임이 목표보다 빠르게 나와도 배열이 넘치지 않도록 여유를 둔다.
        private const int FrameCapacityMargin = 2;

        private enum Phase
        {
            Waiting,   // 메뉴에서 적 수를 고르는 중 (게임 정지)
            Warmup,    // 적을 채우고 안정될 때까지 기다림 (기록 안 함)
            Measuring, // PerfSampler가 기록 중
            Done       // 결과 표시 (게임 정지)
        }

        [SerializeField] private BenchmarkSettings _settings;
        [SerializeField] private PerfOverlay _overlay;
        [SerializeField] private EnemySpawner _spawner;
        [SerializeField] private EnemyRegistry _registry;
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private GameObject _menu;
        [SerializeField] private Button[] _countButtons; // _settings.EnemyCounts와 같은 순서
        [SerializeField] private Button _retryButton;

        private PerfSampler _sampler;
        private Phase _phase = Phase.Waiting;
        private float _warmupTimer;
        private int _enemyCount;
        private long _aliveCountSum; // 측정 중 매 프레임 살아 있는 적 수의 합. 평균을 내서 "정말 유지됐는지" 결과에 남긴다.

        private void Awake()
        {
            Application.targetFrameRate = _settings.TargetFrameRate;
            // 메뉴에서 기다리는 시간만큼 봇이 움직이거나 적이 나오면 매번 시작 상태가 달라진다. 고를 때까지 멈춰 둔다.
            Time.timeScale = PausedTimeScale;

            int frameCapacity = Mathf.CeilToInt(_settings.MeasureSeconds * _settings.TargetFrameRate) * FrameCapacityMargin;
            _sampler = new PerfSampler(frameCapacity);
            _overlay.Bind(_sampler);

            _menu.SetActive(true);
            _retryButton.gameObject.SetActive(false);
            BindButtons();
        }

        private void Update()
        {
            // timeScale이 0이어도 실제 흐른 시간으로 재야 해서 unscaled를 쓴다.
            float deltaTime = Time.unscaledDeltaTime;
            // 측정 구간이 아니어도 매 프레임 넘긴다. 오버레이의 실시간 FPS가 이 값으로 갱신된다.
            _sampler.Tick(deltaTime);

            switch (_phase)
            {
                case Phase.Warmup:
                    _warmupTimer += deltaTime;
                    if (_warmupTimer >= _settings.WarmupSeconds)
                    {
                        BeginMeasuring();
                    }
                    break;

                case Phase.Measuring:
                    _aliveCountSum += _registry.Count;
                    if (_sampler.Elapsed >= _settings.MeasureSeconds)
                    {
                        FinishMeasuring();
                    }
                    break;
            }
        }

        private void OnDestroy()
        {
            // ProfilerRecorder는 네이티브 자원이라 직접 해제해야 한다.
            _sampler?.Dispose();
        }

        private void BindButtons()
        {
            int buttonCount = Mathf.Min(_countButtons.Length, _settings.EnemyCounts.Count);
            if (buttonCount != _countButtons.Length || buttonCount != _settings.EnemyCounts.Count)
            {
                Debug.LogError("BenchmarkRunner: 버튼 수와 BenchmarkSettings의 적 수 개수가 다릅니다.", this);
            }

            for (int i = 0; i < buttonCount; i++)
            {
                // 람다가 반복 변수 i를 직접 잡으면 모든 버튼이 반복이 끝난 뒤의 i를 보게 된다. 지역 변수에 복사해 버튼마다 따로 잡는다.
                int enemyCount = _settings.EnemyCounts[i];
                // 람다는 RemoveListener로 뺄 수 없지만, 버튼과 이 컴포넌트가 같은 씬에서 함께 파괴되므로 해제하지 않는다.
                _countButtons[i].onClick.AddListener(() => StartRun(enemyCount));
            }
            _retryButton.onClick.AddListener(OnRetryClicked);
        }

        private void StartRun(int enemyCount)
        {
            if (_phase != Phase.Waiting)
            {
                return;
            }

            _enemyCount = enemyCount;
            // 메뉴에서 기다린 시간과 상관없이 같은 난수 순서가 되도록, 스폰이 시작되기 직전에 시드를 넣는다.
            Random.InitState(_settings.Seed);
            _playerHealth.SetInvincible(true);
            _spawner.SetMaintainCount(enemyCount, _settings.MaxSpawnPerFrame);
            _spawner.enabled = true;

            _menu.SetActive(false);
            Time.timeScale = NormalTimeScale;
            _warmupTimer = 0f;
            _phase = Phase.Warmup;
        }

        private void BeginMeasuring()
        {
            _aliveCountSum = 0;
            _sampler.Begin();
            _phase = Phase.Measuring;
        }

        private void FinishMeasuring()
        {
            PerfReport report = _sampler.End();
            _phase = Phase.Done;
            // 결과를 보는 동안 기기가 계속 일하지 않게 멈춘다(다음 측정 전 발열을 줄인다).
            Time.timeScale = PausedTimeScale;

            float averageAlive = report.FrameCount > 0 ? (float)_aliveCountSum / report.FrameCount : 0f;
            // 측정이 끝난 뒤라 문자열 할당은 결과에 섞이지 않는다.
            string label = string.Format(CultureInfo.InvariantCulture, "enemies={0} aliveAvg={1:0} seed={2}",
                _enemyCount, averageAlive, _settings.Seed);

            // Release 빌드에서도 logcat(Unity 태그)에 남아서 adb로 결과를 모을 수 있다.
            Debug.Log(report.ToLogLine(label));
            _overlay.ShowReport(report, label);
            _retryButton.gameObject.SetActive(true);
        }

        private void OnRetryClicked()
        {
            // 적·플레이어·난수 상태를 깨끗이 되돌리는 가장 확실한 방법은 씬을 다시 여는 것이다. 새 씬의 Awake가 timeScale을 다시 0으로 둔다.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
