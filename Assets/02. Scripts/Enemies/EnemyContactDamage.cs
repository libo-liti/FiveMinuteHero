using FiveMinuteHero.Combat;
using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 닿아 있는 동안 상대에게 접촉 피해를 준다. 피해 간격(무적 시간)은 맞는 쪽이 관리한다.
    public class EnemyContactDamage : MonoBehaviour
    {
        [SerializeField] private EnemyData _data;

        // 처음 닿은 물리 스텝은 Enter만 불리고 Stay는 그다음 스텝부터라서 둘 다 처리한다.
        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryDamage(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryDamage(collision);
        }

        private void TryDamage(Collision2D collision)
        {
            // 상대가 플레이어인지 묻지 않고, 피해를 받을 수 있는 대상인지만 확인한다.
            if (collision.gameObject.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_data.ContactDamage);
            }
        }
    }
}
