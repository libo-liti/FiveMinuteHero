using System.Collections.Generic;
using FiveMinuteHero.Upgrades;
using UnityEngine;
using UnityEngine.Events;

namespace FiveMinuteHero.UI
{
    // 레벨업 선택 창. 컨트롤러가 보낸 선택지를 카드에 띄우고, 고른 번호만 돌려준다.
    // 정지·후보 뽑기·적용은 모른다(전부 LevelUpController 몫).
    public class LevelUpView : MonoBehaviour
    {
        [SerializeField] private LevelUpController _controller;
        // GameOverView처럼 이 컴포넌트는 항상 켜진 부모에 두고 자식 패널만 끄고 켠다. 자기 자신을 끄면 구독이 풀린다.
        [SerializeField] private GameObject _panel;
        [SerializeField] private LevelUpCardView[] _cards;

        // 카드마다 "몇 번째 카드인지"를 기억하는 리스너. 람다를 매번 새로 만들면 RemoveListener로 뺄 수 없어서 미리 만들어 둔다.
        private UnityAction[] _clickHandlers;

        private void Awake()
        {
            // 씬에서 실수로 켜 둔 채 저장해도 시작할 때는 항상 닫혀 있게 한다.
            _panel.SetActive(false);

            _clickHandlers = new UnityAction[_cards.Length];
            for (int i = 0; i < _cards.Length; i++)
            {
                // 람다가 반복 변수 i를 직접 잡으면 루프가 끝난 뒤의 값을 볼 수 있어서, 반복마다 새 변수에 복사한다.
                int index = i;
                _clickHandlers[i] = () => OnCardClicked(index);
            }
        }

        private void OnEnable()
        {
            _controller.ChoicesOffered += OnChoicesOffered;
            for (int i = 0; i < _cards.Length; i++)
            {
                _cards[i].Button.onClick.AddListener(_clickHandlers[i]);
            }
        }

        private void OnDisable()
        {
            _controller.ChoicesOffered -= OnChoicesOffered;
            for (int i = 0; i < _cards.Length; i++)
            {
                _cards[i].Button.onClick.RemoveListener(_clickHandlers[i]);
            }
        }

        private void OnChoicesOffered(IReadOnlyList<UpgradeOption> options)
        {
            // 남은 강화가 3개 미만이면 남는 카드는 숨긴다.
            for (int i = 0; i < _cards.Length; i++)
            {
                if (i < options.Count)
                {
                    _cards[i].Show(options[i]);
                }
                else
                {
                    _cards[i].Hide();
                }
            }

            _panel.SetActive(true);
        }

        private void OnCardClicked(int index)
        {
            // 먼저 닫고 고른다. 연속 레벨업이면 Choose 안에서 다음 선택지가 바로 와서 같은 프레임에 다시 열린다.
            // 순서를 바꾸면 방금 연 창을 여기서 닫아 버린다.
            _panel.SetActive(false);
            _controller.Choose(index);
        }
    }
}
