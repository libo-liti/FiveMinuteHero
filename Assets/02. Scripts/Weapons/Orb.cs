using System.Collections.Generic;
using FiveMinuteHero.Combat;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 플레이어 주위를 도는 구체 하나. 닿아 있는 대상에게 HitInterval마다 피해를 준다.
    // 투사체와 달리 맞혀도 사라지지 않으므로, 같은 대상을 매 물리 스텝 때리지 않게 대상별 다음 타격 시각을 기억한다.
    // 누구와 닿을지는 레이어 충돌 매트릭스가 정한다(프리팹 레이어: PlayerProjectile).
    public class Orb : MonoBehaviour
    {
        // 지금 닿아 있는 대상만 들어 있다. 나가면 지우므로 죽은 적이 쌓이지 않는다.
        // 키가 Collider2D인 이유: 파괴되는 적은 Exit에서 GetComponent를 할 수 없지만, 받은 Collider2D로 지우는 것은 안전하다.
        private readonly Dictionary<Collider2D, float> _nextHitTimes = new Dictionary<Collider2D, float>();

        private WeaponRuntimeStats _stats;

        // OrbitWeapon이 생성 직후 호출한다. 무기와 같은 스탯 객체를 받아 강화가 바로 반영된다.
        public void Init(WeaponRuntimeStats stats)
        {
            _stats = stats;
        }

        private void Start()
        {
            if (_stats == null)
            {
                Debug.LogError("Orb: Init을 받지 못했습니다. OrbitWeapon의 생성 코드를 확인하세요.", this);
                // 충돌 콜백은 꺼진 컴포넌트에도 오므로 enabled = false로는 부족하다. 오브젝트째로 끈다.
                gameObject.SetActive(false);
            }
        }

        // Enter가 아니라 Stay를 쓴다. 오브에 계속 붙어 있는 적도 간격마다 맞아야 한다.
        private void OnTriggerStay2D(Collider2D other)
        {
            // 간격을 먼저 확인한다. GetComponent 계열은 실제로 때릴 때만 부르면 된다.
            if (_nextHitTimes.TryGetValue(other, out float nextHitTime) && Time.time < nextHitTime)
            {
                return;
            }

            if (!other.TryGetComponent(out IDamageable damageable))
            {
                return;
            }

            // 피해보다 먼저 기록한다. 스탯을 때릴 때마다 읽어서 강화가 바로 반영된다. Time.time은 정지 중에 멈춘다.
            _nextHitTimes[other] = Time.time + _stats.Get(WeaponStatType.HitInterval);
            damageable.TakeDamage(_stats.Damage);
        }

        // Physics2D의 Callbacks On Disable이 켜져 있어서, 적이 파괴될 때도 호출된다.
        private void OnTriggerExit2D(Collider2D other)
        {
            _nextHitTimes.Remove(other);
        }
    }
}
