using FiveMinuteHero.Combat;
using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 닿아 있는 동안 상대에게 접촉 피해를 준다. 피해 간격(무적 시간)은 맞는 쪽이 관리한다.
    public class EnemyContactDamage : MonoBehaviour
    {
        // 피해를 줄 레이어(Player). 콜라이더의 콜백 범위와 피해 대상을 이 값 하나로 함께 정한다.
        [SerializeField] private LayerMask _targetLayers;

        private EnemyData _data;

        // 자기 오브젝트의 콜라이더 설정만 바꾸고 주입값이나 다른 오브젝트에 기대지 않으므로, "Awake는 자기 것만" 규칙의 취지 안이다.
        private void Awake()
        {
            if (!TryGetComponent(out Collider2D hitCollider))
            {
                Debug.LogError("EnemyContactDamage: 같은 오브젝트에 Collider2D가 없어 콜백 범위를 제한하지 못했습니다.", this);
                return;
            }

            // 적끼리 닿을 때마다 콜백이 불렸다가 버려지는 비용을 없앤다. 접촉 자체는 막지 않으므로 서로 밀어내는 물리는 그대로다.
            // 콜백 범위는 이 콜라이더 쪽에만 적용된다. 투사체·오브는 자기 콜라이더 설정대로 적과의 콜백을 계속 받는다.
            hitCollider.callbackLayers = _targetLayers;
        }

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
            // Awake에서 콜백 범위를 줄였으니 여기는 대상 레이어만 온다. 그 설정이 빠진 경우에도 적끼리 때리지 않게 남겨 둔 안전장치다.
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
