using UnityEngine;

namespace FiveMinuteHero.Player
{
    // IMoveInput에서 방향을 받아 Kinematic Rigidbody2D를 MovePosition으로 움직인다. 입력이 키보드인지 봇인지는 모른다.
    // Dynamic이면 적 무리가 밀어붙일 때 갇혀서 못 움직였다. Kinematic은 적에게 밀리지 않고, 적 쪽이 밀려난다.
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
            // 사망 시 PlayerHealth가 이 컴포넌트를 끈다. Kinematic은 남는 속도가 없어서 MovePosition만 멈추면 제자리에 선다.
            // 다시 켜졌을 때 이전 입력으로 한 스텝 움직이지 않도록 방향만 지운다.
            _direction = Vector2.zero;
        }

        private void Update()
        {
            // 어떤 입력 구현이 와도 길이 1을 넘지 않게 여기서 막는다.
            // 대각선 WASD(길이 1.41)는 1로 줄이고, 조이스틱을 살짝 민 값(0.5 등)은 그대로 둔다.
            _direction = Vector2.ClampMagnitude(_moveInput.Direction, 1f);
        }

        private void FixedUpdate()
        {
            // 물리 스텝마다 "이번 스텝에 갈 거리 = 방향 × 초당 속도 × 스텝 시간"만큼 옮긴다.
            // transform을 직접 바꾸지 않고 MovePosition을 써야 물리 엔진이 이동으로 인식해 충돌·보간이 맞게 처리된다.
            _rigidbody.MovePosition(_rigidbody.position + _direction * (_stats.MoveSpeed * Time.fixedDeltaTime));
        }
    }
}
