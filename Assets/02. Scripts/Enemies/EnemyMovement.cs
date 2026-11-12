using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 주입받은 타깃(플레이어)을 향해 Rigidbody2D 속도로 직선 추적한다.
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovement : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private EnemyData _data;
        private Transform _target;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        // Enemy 루트가 생성 직후 호출한다. 적이 플레이어를 직접 찾지 않게 해서 의존을 밖에서 넣는다.
        public void Init(EnemyData data, Transform target)
        {
            _data = data;
            _target = target;
        }

        // 풀에 반납되어 꺼질 때 멈춘 상태로 둔다. 다시 켜진 뒤 첫 FixedUpdate가 속도를 덮어쓰지만, 반납된 적이 속도를 들고 있지 않게 분명히 한다.
        // Awake에서 캐싱한 _rigidbody를 쓰므로 Init이 아니라 여기서 한다(Init은 Awake보다 먼저 올 수 있다는 규칙).
        private void OnDisable()
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (_target == null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                return;
            }

            // 루트 원점이 발이므로 발→발 방향으로 움직인다. 방향만 쓰도록 길이를 1로 맞춘다.
            Vector2 direction = ((Vector2)_target.position - _rigidbody.position).normalized;
            _rigidbody.linearVelocity = direction * _data.MoveSpeed;
        }
    }
}
