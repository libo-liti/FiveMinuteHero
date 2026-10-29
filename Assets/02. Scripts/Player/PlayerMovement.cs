using UnityEngine;

namespace FiveMinuteHero.Player
{
    // IMoveInput에서 방향을 받아 Dynamic Rigidbody2D의 속도를 정한다. 입력이 키보드인지 봇인지는 모른다.
    // 질량이 적과 같으면 적 무리에 갇혀서 Kinematic으로 바꿨었다. 지금은 게임과 벤치마크의 물리 조건을 맞추려고
    // Dynamic으로 되돌렸고, 질량을 적보다 크게 두어 적이 밀어도 덜 밀리게 한다(질량은 씬 인스펙터에서 정한다).
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
            // 사망 시 PlayerHealth가 이 컴포넌트를 끈다. Dynamic은 마지막 속도가 남아서, 꺼지면 FixedUpdate가 더는 덮어쓰지 않아 미끄러진다.
            // 그래서 속도를 직접 0으로 만든다. 방향도 지워서 다시 켜졌을 때 이전 입력으로 한 스텝 움직이지 않게 한다.
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
            // Dynamic 바디는 MovePosition 대신 속도로 움직여야 물리 엔진이 충돌을 풀면서 이동시킨다.
            // 매 스텝 속도를 덮어쓰므로 입력이 0이면 바로 멈추고, 부딪혀서 생긴 속도도 다음 스텝에 남지 않는다.
            // 스텝 시간은 엔진이 곱하므로 여기서 fixedDeltaTime을 곱하지 않는다.
            _rigidbody.linearVelocity = _direction * _stats.MoveSpeed;
        }
    }
}
