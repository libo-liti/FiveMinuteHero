using FiveMinuteHero.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FiveMinuteHero.UI
{
    // 레벨업 카드 한 장. 받은 선택지를 글자와 아이콘으로 보여 주기만 한다. 클릭 처리는 LevelUpView가 한다.
    public class LevelUpCardView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _levelText;

        public Button Button => _button;

        public void Show(UpgradeOption option)
        {
            UpgradeData data = option.Data;
            _nameText.text = data.DisplayName;
            _descriptionText.text = data.GetDescription(option.NextLevel);
            _levelText.text = $"Lv.{option.NextLevel}";

            // 아이콘이 없는 강화는 칸을 숨긴다. 카드의 레이아웃 그룹이 꺼진 칸을 빼고 글자를 왼쪽으로 붙인다.
            bool hasIcon = data.Icon != null;
            _icon.gameObject.SetActive(hasIcon);
            if (hasIcon)
            {
                _icon.sprite = data.Icon;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
