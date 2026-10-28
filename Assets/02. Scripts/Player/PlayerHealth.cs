using System;
using FiveMinuteHero.Combat;
using UnityEngine;

namespace FiveMinuteHero.Player
{
    // 플레이어 체력. 피격 후 무적 시간 동안은 피해를 무시하고, 체력 변화와 사망을 이벤트로 알린다.
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerStats _stats;
        [SerializeField] private PlayerMovement _movement;

        private int _currentHealth;
        private float _invincibleUntil; // 이 시각(Time.time)까지는 무적
        private bool _isInvincible;     // 벤치마크처럼 죽으면 안 되는 상황에서 켠다

        // (현재 체력, 최대 체력). 체력바 UI 등이 구독한다.
        public event Action<int, int> HealthChanged;
        // 게임오버 처리 쪽이 구독한다. PlayerHealth는 그 다음 일을 모른다.
        public event Action Died;

        public bool IsDead => _currentHealth <= 0;

        private void Awake()
        {
            _currentHealth = _stats.MaxHealth;
        }

        // 측정 중에 죽으면 GameSession이 게임을 멈춰 측정이 망가진다. 그래서 측정하는 쪽이 무적을 켠다.
        public void SetInvincible(bool isInvincible)
        {
            _isInvincible = isInvincible;
        }

        public void TakeDamage(int amount)
        {
            // 적 여러 마리가 동시에 닿아도 무적 시간마다 한 번만 맞는다.
            if (_isInvincible || IsDead || Time.time < _invincibleUntil)
            {
                return;
            }

            _currentHealth = Mathf.Max(_currentHealth - amount, 0);
            _invincibleUntil = Time.time + _stats.InvincibleDuration;
            HealthChanged?.Invoke(_currentHealth, _stats.MaxHealth);

            if (IsDead)
            {
                // 이동을 끈다. Kinematic이라 남는 속도가 없어 그 자리에 멈춘다.
                _movement.enabled = false;
                Died?.Invoke();
            }
        }
    }
}
