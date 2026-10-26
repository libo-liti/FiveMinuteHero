using FiveMinuteHero.Combat;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 직선으로 날아가 처음 닿은 대상에게 피해를 주고 사라진다. 누구와 닿을지는 레이어 충돌 매트릭스가 정한다.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private int _damage;
        private bool _hasHit;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        // 무기가 생성 직후 호출한다. 수명이 지나면 맞히지 못해도 스스로 사라진다.
        public void Launch(Vector2 direction, float speed, int damage, float lifetime)
        {
            _damage = damage;
            _rigidbody.linearVelocity = direction * speed;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 같은 물리 스텝에 적 두 마리와 겹쳐도 한 마리만 맞힌다. Destroy는 프레임 끝에 처리되기 때문이다.
            if (_hasHit || !other.TryGetComponent(out IDamageable damageable))
            {
                return;
            }

            _hasHit = true;
            damageable.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
