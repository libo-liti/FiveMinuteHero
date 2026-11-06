using System;
using FiveMinuteHero.Combat;
using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 적 체력. 맞으면 Damaged, 죽으면 Died로 알리기만 하고, 표시와 오브젝트 제거는 다른 컴포넌트가 맡는다.
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        private int _currentHealth;

        // 피해가 실제로 들어갔을 때 알린다. 이미 죽은 뒤의 타격에는 나가지 않는다.
        public event Action Damaged;
        public event Action Died;

        public bool IsDead => _currentHealth <= 0;

        // Enemy 루트가 생성 직후 호출한다.
        public void Init(EnemyData data)
        {
            _currentHealth = data.MaxHealth;
        }

        public void TakeDamage(int amount)
        {
            // 같은 물리 스텝에 탄 두 발이 맞아도 Died는 한 번만 나가게 한다.
            if (IsDead)
            {
                return;
            }

            _currentHealth = Mathf.Max(_currentHealth - amount, 0);

            // Died보다 먼저 보낸다. 구독자가 Died에서 정리되기 전에 피격 신호를 받을 수 있다.
            Damaged?.Invoke();

            if (IsDead)
            {
                Died?.Invoke();
            }
        }
    }
}
