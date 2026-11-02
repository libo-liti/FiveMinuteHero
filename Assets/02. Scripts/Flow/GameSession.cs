using FiveMinuteHero.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FiveMinuteHero.Flow
{
    // 한 판의 흐름(정지, 게임 오버, 재시작)을 맡는다. 화면은 모르고, 게임 오버 화면이 Restart를 호출한다.
    // 씬에서 timeScale을 바꾸는 곳은 여기 하나다. 정지 요청이 여럿 겹치거나 게임 오버와 겹쳐도 서로 덮어쓰지 않게 하기 위해서다.
    public class GameSession : MonoBehaviour, IPauseService
    {
        private const float PausedTimeScale = 0f;
        private const float NormalTimeScale = 1f;

        [SerializeField] private PlayerHealth _playerHealth;

        private int _pauseRequestCount; // 아직 해제되지 않은 정지 요청 수. bool이면 먼저 닫은 쪽이 다른 쪽 정지까지 풀어 버린다.
        private bool _isGameOver;       // 한 번 켜지면 정지 요청을 다 해제해도 재개하지 않는다. 씬을 다시 불러야 초기화된다.

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

        private void OnPlayerDied()
        {
            _isGameOver = true;
            ApplyTimeScale();
        }

        // 정지 여부는 이 한 곳에서만 정한다.
        // deltaTime 기반 타이머(스폰·무기 쿨타임)와 물리(적 이동·밀림)가 한 번에 멈춘다. UI 입력은 영향을 받지 않는다.
        private void ApplyTimeScale()
        {
            bool isPaused = _isGameOver || _pauseRequestCount > 0;
            Time.timeScale = isPaused ? PausedTimeScale : NormalTimeScale;
        }
    }
}
