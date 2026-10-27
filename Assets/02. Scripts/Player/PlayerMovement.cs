using UnityEngine;

namespace FiveMinuteHero.Player
{
    // IMoveInput에서 방향을 받아 Rigidbody2D 속도로 플레이어를 움직인다. 입력이 키보드인지 봇인지는 모른다.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;
        // Unity는 인터페이스 타입 필드를 직렬화하지 못해서 MonoBehaviour로 받고, Awake에서 IMoveInput으로 바꾼다.
        [SerializeField] private MonoBehaviour _inputSource;

        private Rigidbody2D _rigidbody;
        private IMoveInput _moveInput;
        private Vector2 _direction;

        // 애니메이션 등에서 이동 방향을 읽을 때 쓴다.
        public Vector2 MoveDirection => _direction;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _moveInput = _inputSource as IMoveInput;

            // _inputSource는 Unity의 ==로 비교해야 지워진 컴포넌트(Missing)까지 잡힌다. 캐스팅 결과로는 엉뚱한 컴포넌트를 잡는다.
            if (_inputSource == null || _moveInput == null)
            {
                Debug.LogError("PlayerMovement: _inputSource에 IMoveInput을 구현한 컴포넌트를 연결하세요.", this);
                // 꺼 두면 Update가 돌지 않아 매 프레임 NullReference가 나지 않는다.
                enabled = false;
            }
        }

        private void OnDisable()
        {
            // 사망 시 PlayerHealth가 이 컴포넌트를 끈다. 속도를 지우지 않으면 마지막 속도로 계속 미끄러진다.
            _direction = Vector2.zero;
            _rigidbody.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            // 어떤 입력 구현이 와도 길이 1을 넘지 않게 여기서 막는다.
            // 대각선 WASD(길이 1.41)는 1로 줄이고, 조이스틱을 살짝 민 값(0.5 등)은 그대로 둔다.
            _direction = Vector2.ClampMagnitude(_moveInput.Direction, 1f);
        }

        private void FixedUpdate()
        {
            // 물리 갱신 주기에 맞춰 속도를 넣는다. 위치 계산과 충돌은 물리 엔진이 처리한다.
            _rigidbody.linearVelocity = _direction * _stats.MoveSpeed;
        }
    }
}
