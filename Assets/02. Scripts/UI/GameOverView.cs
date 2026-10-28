using FiveMinuteHero.Flow;
using FiveMinuteHero.Player;
using UnityEngine;
using UnityEngine.UI;

namespace FiveMinuteHero.UI
{
    // 게임 오버 패널. 사망 이벤트를 받아 패널을 켜고, 버튼 입력을 GameSession에 넘긴다. 정지·재시작 규칙은 모른다.
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private GameSession _gameSession;
        // 이 컴포넌트는 항상 켜진 부모에 두고 자식 패널만 끄고 켠다. 자기 자신을 끄면 OnEnable 구독이 안 된다.
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private Button _restartButton;

        private void Awake()
        {
            // 씬에서 실수로 켜 둔 채 저장해도 시작할 때는 항상 닫혀 있게 한다.
            _gameOverPanel.SetActive(false);
        }

        private void OnEnable()
        {
            _playerHealth.Died += OnPlayerDied;
            // 람다 대신 메서드를 넘겨야 OnDisable에서 같은 대상을 RemoveListener로 뺄 수 있다.
            _restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnDisable()
        {
            _playerHealth.Died -= OnPlayerDied;
            _restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        private void OnPlayerDied()
        {
            _gameOverPanel.SetActive(true);
        }

        private void OnRestartClicked()
        {
            _gameSession.Restart();
        }
    }
}
