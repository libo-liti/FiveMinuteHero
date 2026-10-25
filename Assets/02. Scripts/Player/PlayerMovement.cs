using UnityEngine;
using UnityEngine.InputSystem;

namespace FiveMinuteHero.Player
{
    // Move 액션(WASD + 가상 조이스틱)의 값을 읽어 Rigidbody2D 속도로 플레이어를 움직인다.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private PlayerStats _stats;

        private Rigidbody2D _rigidbody;
        private Vector2 _input;

        // 애니메이션 등에서 이동 방향을 읽을 때 쓴다.
        public Vector2 MoveInput => _input;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _moveAction.action.Enable();
        }

        private void OnDisable()
        {
            _moveAction.action.Disable();
            _input = Vector2.zero;
            _rigidbody.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            // 대각선 WASD(길이 1.41)는 1로 줄이고, 조이스틱을 살짝 민 값(0.5 등)은 그대로 둔다.
            _input = Vector2.ClampMagnitude(_moveAction.action.ReadValue<Vector2>(), 1f);
        }

        private void FixedUpdate()
        {
            // 물리 갱신 주기에 맞춰 속도를 넣는다. 위치 계산과 충돌은 물리 엔진이 처리한다.
            _rigidbody.linearVelocity = _input * _stats.MoveSpeed;
        }
    }
}
