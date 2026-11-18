using System;
using System.Collections.Generic;
using FiveMinuteHero.Stages;
using UnityEngine;

namespace FiveMinuteHero.Flow
{
    // 한 판의 시간을 잰다. 남은 초가 바뀔 때 알리고, 판 길이를 다 버티면 판 종료(클리어)를 요청한다.
    // 경과 시간에 맞는 웨이브를 골라 IWaveTarget(스포너)에 넣는다. 시간 기준을 가진 곳이 여기뿐이라 웨이브 판단도 여기서 한다.
    // Main 씬에만 둔다(GameCore 프리팹 밖). Benchmark에는 없어서 타이머·클리어·웨이브가 측정에 끼어들지 않는다.
    // 정지(timeScale)는 모른다. Time.deltaTime으로 재므로 레벨업 창이나 판 종료로 멈추면 시간도 저절로 멈춘다.
    public class StageRunner : MonoBehaviour
    {
        [SerializeField] private StageData _stageData;

        private IRunEndService _runEnd;
        private IWaveTarget _waveTarget;
        private int _waveIndex;            // 지금 넣어 둔 웨이브의 번호(_stageData.Waves 기준)
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
        // 스포너도 IWaveTarget으로만 받는다. 웨이브를 넣는 일 말고는 스포너의 다른 기능(시드·유지 모드)을 부를 수 없다.
        public void Init(IRunEndService runEnd, IWaveTarget waveTarget)
        {
            _runEnd = runEnd;
            _waveTarget = waveTarget;
        }

        private void Start()
        {
            if (_runEnd == null || _waveTarget == null || _stageData == null)
            {
                Debug.LogError("StageRunner: Init을 받지 못했거나 StageData가 비어 있습니다. 씬의 Bootstrap 연결을 확인하세요.", this);
                // 꺼 두면 Update가 돌지 않아 null 참조 에러가 매 프레임 쌓이지 않는다.
                enabled = false;
                return;
            }

            if (!HasValidWaves())
            {
                enabled = false;
                return;
            }

            // 시작 값은 화면이 Start에서 RemainingSeconds를 직접 읽어 그린다. 여기서는 그 값을 "이미 알린 값"으로 둔다.
            _lastRemainingSeconds = RemainingSeconds;

            // 첫 웨이브(0초)도 바뀔 때를 기다리지 않고 여기서 넣는다. 스포너는 웨이브를 받기 전엔 스폰하지 않는다.
            // Start는 씬의 모든 Awake 뒤에 오므로 스포너의 풀·사전은 이미 준비돼 있다.
            _waveIndex = 0;
            _waveTarget.ApplyWave(_stageData.Waves[0]);
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

            AdvanceWaveIfDue();
            NotifyIfSecondChanged();
        }

        // 다음 웨이브 시작 시각을 넘었으면 넘어간다. if가 아니라 while인 이유: 프레임 하나가 길어(로딩 끊김 등)
        // 경계 두 개를 한 번에 넘으면 if로는 한 칸만 가서 한 프레임 동안 이미 끝난 웨이브로 돈다.
        // 넘어간 칸이 여럿이어도 스포너에는 마지막 웨이브 하나만 넣는다(중간 웨이브는 0초 동안이라 의미가 없다).
        private void AdvanceWaveIfDue()
        {
            IReadOnlyList<WaveData> waves = _stageData.Waves;
            int previousIndex = _waveIndex;
            while (_waveIndex + 1 < waves.Count && _elapsedSeconds >= waves[_waveIndex + 1].StartSeconds)
            {
                _waveIndex++;
            }

            if (_waveIndex != previousIndex)
            {
                _waveTarget.ApplyWave(waves[_waveIndex]);
            }
        }

        // 웨이브 목록이 이 클래스가 기대하는 모양인지 판 시작 때 한 번 검사한다. 데이터 실수는 조용히 넘기지 않고 알린다.
        private bool HasValidWaves()
        {
            IReadOnlyList<WaveData> waves = _stageData.Waves;
            if (waves == null || waves.Count == 0)
            {
                Debug.LogError($"StageRunner: {_stageData.name}에 웨이브가 없습니다.", this);
                return false;
            }

            // 0초가 아니면 첫 구간에 넣을 웨이브가 없다.
            if (waves[0].StartSeconds > 0f)
            {
                Debug.LogError($"StageRunner: {_stageData.name}의 첫 웨이브가 0초에 시작하지 않습니다({waves[0].StartSeconds}초).", this);
                return false;
            }

            // AdvanceWaveIfDue는 "다음 칸"만 보므로, 순서가 섞이면 뒤 웨이브를 건너뛰거나 영영 못 간다.
            for (int i = 1; i < waves.Count; i++)
            {
                if (waves[i].StartSeconds <= waves[i - 1].StartSeconds)
                {
                    Debug.LogError($"StageRunner: {_stageData.name}의 웨이브 시작 시각이 오름차순이 아닙니다({i - 1}번 {waves[i - 1].StartSeconds}초, {i}번 {waves[i].StartSeconds}초).", this);
                    return false;
                }
            }

            return true;
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
