using System;
using FiveMinuteHero.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FiveMinuteHero.Flow
{
    // 한 판의 흐름(정지, 판 종료, 재시작)을 맡는다. 화면은 모르고, 결과 화면이 Restart를 호출한다.
    // 씬에서 timeScale을 바꾸는 곳은 여기 하나다. 정지 요청이 여럿 겹치거나 판 종료와 겹쳐도 서로 덮어쓰지 않게 하기 위해서다.
    // 판이 끝나는 길(사망, 클리어)도 여기 한 곳으로 모은다. 클리어를 알리는 쪽(StageRunner)은 이 클래스가 모르고, IRunEndService로 요청만 받는다.
    public class GameSession : MonoBehaviour, IPauseService, IRunEndService, IRunState
    {
        private const float PausedTimeScale = 0f;
        private const float NormalTimeScale = 1f;

        [SerializeField] private PlayerHealth _playerHealth;

        private int _pauseRequestCount; // 아직 해제되지 않은 정지 요청 수. bool이면 먼저 닫은 쪽이 다른 쪽 정지까지 풀어 버린다.
        private bool _isRunOver;        // 래치: 한 번 켜지면 꺼지지 않는다. 정지 요청을 다 해제해도 재개하지 않고, 두 번째 종료 요청도 막는다. 씬을 다시 불러야 초기화된다.

        public bool IsRunOver => _isRunOver;
        // 결과 화면, 레벨업 창 등이 구독한다. 어떻게 끝났는지 함께 넘긴다.
        public event Action<RunOutcome> RunEnded;

        private void OnEnable()
        {
            _playerHealth.Died += OnPlayerDied;
        }

        private void OnDisable()
        {
            _playerHealth.Died -= OnPlayerDied;
        }

        public void Restart()
        {
            // timeScale은 씬에 속하지 않는 static 값이라 리로드해도 유지된다. 로드 전에 되돌리지 않으면 새 판이 멈춘 채 시작한다.
            Time.timeScale = NormalTimeScale;
            // 씬 이름을 문자열로 적지 않고 지금 씬을 다시 불러온다. 체력·적·젬은 새 오브젝트가 되어 처음 상태로 돌아간다.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void RequestPause()
        {
            _pauseRequestCount++;
            ApplyTimeScale();
        }

        public void ReleasePause()
        {
            // 요청보다 해제가 많으면 어딘가 짝이 어긋난 것이다. 음수가 되면 다음 요청 하나로는 멈추지 않으므로 여기서 막는다.
            if (_pauseRequestCount <= 0)
            {
                Debug.LogError("GameSession: 정지 요청 없이 ReleasePause가 호출됐습니다.", this);
                return;
            }

            _pauseRequestCount--;
            ApplyTimeScale();
        }

        public void EndRun(RunOutcome outcome)
        {
            // 클리어와 사망이 같은 프레임에 겹치면 먼저 온 쪽만 결과가 된다. 검사를 맨 앞에 둬야 두 번째 요청이 아무것도 바꾸지 못한다.
            if (_isRunOver)
            {
                return;
            }

            _isRunOver = true;
            ApplyTimeScale();
            // 멈춘 뒤에 알린다. 구독자(결과 화면)는 이미 멈춘 상태에서 수치를 읽으므로, 읽는 사이에 값이 더 바뀌지 않는다.
            RunEnded?.Invoke(outcome);
        }

        // 사망은 지금처럼 여기서 직접 받는다. 클리어와 같은 EndRun 경로를 타서 래치를 함께 쓴다.
        private void OnPlayerDied()
        {
            EndRun(RunOutcome.Failed);
        }

        // 정지 여부는 이 한 곳에서만 정한다.
        // deltaTime 기반 타이머(스폰·무기 쿨타임)와 물리(적 이동·밀림)가 한 번에 멈춘다. UI 입력은 영향을 받지 않는다.
        private void ApplyTimeScale()
        {
            bool isPaused = _isRunOver || _pauseRequestCount > 0;
            Time.timeScale = isPaused ? PausedTimeScale : NormalTimeScale;
        }
    }
}
