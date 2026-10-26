using FiveMinuteHero.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FiveMinuteHero.UI
{
    // 화면 위 경험치 바. PlayerExperience의 이벤트만 구독해서 그리고, 경험치 계산은 하지 않는다.
    public class ExpBarView : MonoBehaviour
    {
        [SerializeField] private PlayerExperience _experience;
        [SerializeField] private Image _fill; // Image Type = Filled, Horizontal
        [SerializeField] private TMP_Text _levelText;

        private void OnEnable()
        {
            _experience.ExpChanged += OnExpChanged;
            _experience.LeveledUp += OnLeveledUp;
        }

        private void OnDisable()
        {
            _experience.ExpChanged -= OnExpChanged;
            _experience.LeveledUp -= OnLeveledUp;
        }

        private void Start()
        {
            // 이벤트는 경험치가 처음 바뀔 때까지 안 오므로, 시작 상태(Lv.1, 0/필요량)는 직접 한 번 그린다.
            OnExpChanged(_experience.CurrentExp, _experience.RequiredExp);
            OnLeveledUp(_experience.Level);
        }

        private void OnExpChanged(int current, int required)
        {
            // int끼리 나누면 0이 되므로 float로 바꿔 나눈다.
            _fill.fillAmount = (float)current / required;
        }

        private void OnLeveledUp(int level)
        {
            _levelText.text = $"Lv.{level}";
        }
    }
}
