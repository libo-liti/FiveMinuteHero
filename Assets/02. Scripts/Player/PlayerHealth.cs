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

        // (현재 체력, 최대 체력). 체력바 UI 등이 구독한다.
        public event Action<int, int> HealthChanged;
        // 게임오버 처리 쪽이 구독한다. PlayerHealth는 그 다음 일을 모른다.
        public event Action Died;

        public bool IsDead => _currentHealth <= 0;

        private void Awake()
        {
            _currentHealth = _stats.MaxHealth;
        }

        public void TakeDamage(int amount)
        {
            // 적 여러 마리가 동시에 닿아도 무적 시간마다 한 번만 맞는다.
            if (IsDead || Time.time < _invincibleUntil)
            {
                return;
            }

            _currentHealth = Mathf.Max(_currentHealth - amount, 0);
            _invincibleUntil = Time.time + _stats.InvincibleDuration;
            Debug.Log($"[PlayerHealth] 피해 {amount}, 남은 체력 {_currentHealth}/{_stats.MaxHealth}");
            HealthChanged?.Invoke(_currentHealth, _stats.MaxHealth);

            if (IsDead)
            {
                // PlayerMovement.OnDisable이 입력과 속도를 0으로 정리한다.
                _movement.enabled = false;
                Died?.Invoke();
            }
        }
    }
}
