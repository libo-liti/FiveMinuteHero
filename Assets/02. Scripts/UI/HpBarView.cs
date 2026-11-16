using FiveMinuteHero.Player;
using UnityEngine;

namespace FiveMinuteHero.UI
{
    // 플레이어 발 아래 체력바. PlayerHealth의 이벤트만 구독해서 그리고, 체력 계산은 하지 않는다.
    // Canvas 대신 SpriteRenderer 두 장(배경 + 채움)을 쓰고, 채움의 scale.x를 아트 픽셀 수로 맞춰 픽셀 화면에서 선명하게 한다.
    // 좌우 반전은 Visual에서만 한다. 플레이어 루트의 scale.x를 뒤집으면 이 바도 뒤집혀 줄어드는 방향이 반대가 된다.
    public class HpBarView : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _health;
        // 피벗이 왼쪽인 1×1px(PPU 16) 스프라이트. 프리팹에서 가득 찬 길이로 둔다. scale.x 1 = 아트 1픽셀.
        [SerializeField] private Transform _fill;

        private float _fullWidth; // 가득 찼을 때 채움 길이(픽셀). 프리팹 값을 읽어 와서 코드에 숫자를 따로 적지 않는다.

        private void Awake()
        {
            _fullWidth = _fill.localScale.x;
        }

        private void OnEnable()
        {
            _health.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            _health.HealthChanged -= OnHealthChanged;
        }

        private void Start()
        {
            // 이벤트는 체력이 처음 바뀔 때까지 안 오므로, 시작 상태(가득 참)는 직접 한 번 그린다.
            OnHealthChanged(_health.CurrentHealth, _health.MaxHealth);
        }

        private void OnHealthChanged(int current, int max)
        {
            // int끼리 나누면 0이 되므로 float로 바꿔 나눈다.
            float ratio = (float)current / max;
            // 소수 픽셀은 가장자리가 뭉개지므로 정수 픽셀로 맞춘다.
            // 내림이 아니라 올림인 이유: 체력이 조금이라도 남았으면 최소 1픽셀은 보여야 죽은 것과 구분된다. 0일 때만 0이 된다.
            float width = Mathf.Ceil(ratio * _fullWidth);

            Vector3 scale = _fill.localScale;
            scale.x = width;
            _fill.localScale = scale;
        }
    }
}
