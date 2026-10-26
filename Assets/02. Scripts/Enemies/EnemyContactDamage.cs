using FiveMinuteHero.Combat;
using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 닿아 있는 동안 상대에게 접촉 피해를 준다. 피해 간격(무적 시간)은 맞는 쪽이 관리한다.
    public class EnemyContactDamage : MonoBehaviour
    {
        // 피해를 줄 레이어(Player). 적끼리도 충돌하므로 이걸로 거르지 않으면 서로 때려 죽인다.
        [SerializeField] private LayerMask _targetLayers;

        private EnemyData _data;

        // Enemy 루트가 생성 직후 호출한다.
        public void Init(EnemyData data)
        {
            _data = data;
        }

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
            // 레이어 번호를 비트 위치로 바꿔 마스크에 켜져 있는지 본다. 대상 레이어가 아니면 무시한다.
            if ((_targetLayers.value & (1 << collision.gameObject.layer)) == 0)
            {
                return;
            }

            // 대상 레이어 안에서는 종류를 묻지 않고, 피해를 받을 수 있는 대상인지만 확인한다.
            if (collision.gameObject.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_data.ContactDamage);
            }
        }
    }
}
