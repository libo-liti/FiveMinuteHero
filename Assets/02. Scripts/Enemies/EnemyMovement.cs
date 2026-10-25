using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 주입받은 타깃(플레이어)을 향해 Rigidbody2D 속도로 직선 추적한다.
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private EnemyData _data;

        private Rigidbody2D _rigidbody;
        private Transform _target;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        // 스포너가 생성 직후 호출한다. 적이 플레이어를 직접 찾지 않게 해서 의존을 밖에서 넣는다.
        public void Init(Transform target)
        {
            _target = target;
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
