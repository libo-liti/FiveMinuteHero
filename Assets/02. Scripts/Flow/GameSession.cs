using FiveMinuteHero.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FiveMinuteHero.Flow
{
    // 한 판의 흐름(게임 오버 시 정지, 재시작)을 맡는다. 화면은 모르고, 게임 오버 화면이 Restart를 호출한다.
    public class GameSession : MonoBehaviour
    {
        private const float PausedTimeScale = 0f;
        private const float NormalTimeScale = 1f;

        [SerializeField] private PlayerHealth _playerHealth;

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

        private void OnPlayerDied()
        {
            // deltaTime 기반 타이머(스폰·무기 쿨타임)와 물리(적 이동·밀림)가 한 번에 멈춘다. UI 입력은 영향을 받지 않는다.
            Time.timeScale = PausedTimeScale;
        }
    }
}
