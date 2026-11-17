using FiveMinuteHero.Enemies;
using FiveMinuteHero.Flow;
using FiveMinuteHero.Player;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FiveMinuteHero.UI
{
    // 결과 패널(클리어·실패 공용). 판 종료 이벤트를 받아 그 순간의 수치를 써 두고 패널을 켠다.
    // 버튼 입력은 GameSession에 넘긴다. 정지·재시작·판정 규칙은 모른다.
    public class RunResultView : MonoBehaviour
    {
        private const int SecondsPerMinute = 60;
        private const string TimeFormat = "{0}:{1:00}";
        private const string KillCountFormat = "{0}";
        private const string LevelFormat = "Lv.{0}";

        [SerializeField] private GameSession _gameSession;
        // 수치를 읽어 올 곳. Main에만 있는 StageRunner를 참조하므로 이 화면도 Main에만 둔다.
        [SerializeField] private StageRunner _stageRunner;
        [SerializeField] private EnemyRegistry _enemyRegistry;
        [SerializeField] private PlayerExperience _experience;

        // 이 컴포넌트는 항상 켜진 부모에 두고 자식 패널만 끄고 켠다. 자기 자신을 끄면 OnEnable 구독이 안 된다.
        // 필드 이름을 바꿔도 씬에 저장된 기존 연결(_gameOverPanel)을 이어받는다.
        [FormerlySerializedAs("_gameOverPanel")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _survivalTimeText;
        [SerializeField] private TMP_Text _killCountText;
        [SerializeField] private TMP_Text _levelText;

        // 결과마다 다른 제목과 색. 문구·색을 바꿀 때 코드를 고치지 않게 인스펙터에 둔다.
        [SerializeField] private string _clearedTitle = "STAGE CLEAR";
        [SerializeField] private Color _clearedColor = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private string _failedTitle = "GAME OVER";
        [SerializeField] private Color _failedColor = new Color(0.9f, 0.25f, 0.25f);

        private void Awake()
        {
            // 씬에서 실수로 켜 둔 채 저장해도 시작할 때는 항상 닫혀 있게 한다.
            _panel.SetActive(false);
        }

        private void OnEnable()
        {
            _gameSession.RunEnded += OnRunEnded;
            // 람다 대신 메서드를 넘겨야 OnDisable에서 같은 대상을 RemoveListener로 뺄 수 있다.
            _restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnDisable()
        {
            _gameSession.RunEnded -= OnRunEnded;
            _restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        private void OnRunEnded(RunOutcome outcome)
        {
            bool isCleared = outcome == RunOutcome.Cleared;
            _titleText.text = isCleared ? _clearedTitle : _failedTitle;
            _titleText.color = isCleared ? _clearedColor : _failedColor;

            // 스냅샷: 판이 끝난 이 순간의 값을 한 번만 써 둔다. 이후 정지 중에 젬이 흡수돼 레벨이 올라도 결과는 바뀌지 않는다.
            // GameSession이 멈춘 뒤에 이 이벤트를 보내므로, 읽는 값은 이미 멈춘 판의 값이다.
            // 클리어면 StageRunner가 판 길이로 잘라 두어서 정확히 5:00이 나온다.
            int survivalSeconds = Mathf.FloorToInt(_stageRunner.ElapsedSeconds);
            _survivalTimeText.SetText(TimeFormat, survivalSeconds / SecondsPerMinute, survivalSeconds % SecondsPerMinute);
            _killCountText.SetText(KillCountFormat, _enemyRegistry.DiedCount);
            _levelText.SetText(LevelFormat, _experience.Level);

            _panel.SetActive(true);
        }

        private void OnRestartClicked()
        {
            _gameSession.Restart();
        }
    }
}
