using System;
using FiveMinuteHero.Combat;
using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 적 체력. 죽으면 Died로 알리기만 하고, 오브젝트 제거는 Enemy 루트가 맡는다.
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        private int _currentHealth;

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

            if (IsDead)
            {
                Died?.Invoke();
            }
        }
    }
}
