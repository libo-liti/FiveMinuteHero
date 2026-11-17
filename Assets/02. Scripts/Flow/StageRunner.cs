using System;
using FiveMinuteHero.Stages;
using UnityEngine;

namespace FiveMinuteHero.Flow
{
    // 한 판의 시간을 잰다. 남은 초가 바뀔 때 알리고, 판 길이를 다 버티면 판 종료(클리어)를 요청한다.
    // Main 씬에만 둔다(GameCore 프리팹 밖). Benchmark에는 없어서 타이머·클리어가 측정에 끼어들지 않는다.
    // 정지(timeScale)는 모른다. Time.deltaTime으로 재므로 레벨업 창이나 판 종료로 멈추면 시간도 저절로 멈춘다.
    public class StageRunner : MonoBehaviour
    {
        [SerializeField] private StageData _stageData;

        private IRunEndService _runEnd;
        private float _elapsedSeconds;
        private int _lastRemainingSeconds; // 마지막으로 알린 남은 초. 같은 값이면 알리지 않아 화면이 초마다 한 번만 갱신된다.
        private bool _isCleared;           // 클리어 요청은 한 번만 보낸다

        // 남은 초가 바뀔 때마다 알린다(남은 초). 타이머 화면이 구독한다.
        public event Action<int> RemainingSecondsChanged;

        // 결과 화면이 생존 시간으로 읽는다.
        public float ElapsedSeconds => _elapsedSeconds;
        // 올림이라 시작할 때 300(5:00)이고, 0이 되는 순간이 판 길이에 닿은 순간이다.
        public int RemainingSeconds => Mathf.CeilToInt(_stageData.DurationSeconds - _elapsedSeconds);

        // Bootstrap이 호출한다. GameSession은 이 클래스를 모르고, 여기서 IRunEndService로만 판 종료를 요청한다.
        public void Init(IRunEndService runEnd)
        {
            _runEnd = runEnd;
        }

        private void Start()
        {
            if (_runEnd == null || _stageData == null)
            {
                Debug.LogError("StageRunner: Init을 받지 못했거나 StageData가 비어 있습니다. 씬의 Bootstrap 연결을 확인하세요.", this);
                // 꺼 두면 Update가 돌지 않아 null 참조 에러가 매 프레임 쌓이지 않는다.
                enabled = false;
                return;
            }

            // 시작 값은 화면이 Start에서 RemainingSeconds를 직접 읽어 그린다. 여기서는 그 값을 "이미 알린 값"으로 둔다.
            _lastRemainingSeconds = RemainingSeconds;
        }

        private void Update()
        {
            if (_isCleared)
            {
                return;
            }

            _elapsedSeconds += Time.deltaTime;

            // 프레임 시간만큼 판 길이를 살짝 넘을 수 있어서 판 길이에서 자른다. 그래야 생존 시간이 5:00으로 딱 맞게 나온다.
            if (_elapsedSeconds >= _stageData.DurationSeconds)
            {
                _elapsedSeconds = _stageData.DurationSeconds;
                _isCleared = true;
                NotifyIfSecondChanged(); // 0:00을 먼저 그린 뒤 결과 화면이 뜬다
                _runEnd.EndRun(RunOutcome.Cleared);
                return;
            }

            NotifyIfSecondChanged();
        }

        private void NotifyIfSecondChanged()
        {
            int remaining = RemainingSeconds;
            if (remaining == _lastRemainingSeconds)
            {
                return;
            }

            _lastRemainingSeconds = remaining;
            RemainingSecondsChanged?.Invoke(remaining);
        }
    }
}
