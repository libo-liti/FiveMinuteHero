using System;
using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 적 프리팹의 루트. EnemyData를 한 곳에서만 들고, 하위 컴포넌트에 Init으로 나눠 준다.
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyData _data;
        [SerializeField] private EnemyMovement _movement;
        [SerializeField] private EnemyContactDamage _contactDamage;
        [SerializeField] private EnemyHealth _health;
        [SerializeField] private Collider2D _hitCollider;

        // 점수·경험치 등이 구독한다. 어떤 적이 죽었는지 함께 넘긴다.
        public event Action<Enemy> Died;

        // 원점은 발이라서, 조준은 몸통(콜라이더 중심)을 향하게 한다.
        public Vector2 AimPoint => _hitCollider.bounds.center;
        // 젬 드롭 쪽이 읽는다. EnemyData 전체를 내주지 않고 필요한 값만 연다.
        public int ExpReward => _data.ExpReward;

        private void Awake()
        {
            _health.Died += OnHealthDied;
        }

        // 스포너가 생성 직후 호출한다. 하위 컴포넌트는 Enemy를 모르고, 받은 값만 쓴다.
        public void Init(Transform target)
        {
            _movement.Init(_data, target);
            _contactDamage.Init(_data);
            _health.Init(_data);
        }

        private void OnHealthDied()
        {
            // 구독자에게 먼저 알린 뒤 제거한다. Destroy는 프레임 끝에 처리되어 구독자가 이 Enemy를 아직 읽을 수 있다.
            Died?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
