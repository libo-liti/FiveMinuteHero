using FiveMinuteHero.Enemies;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 모든 무기의 공통 부분: 주입받기와 주입 확인. 어떻게 공격할지는 하위 클래스가 정한다.
    // 무기를 만드는 쪽은 이 타입만 알면 되므로, 무기 종류가 늘어도 생성 코드를 고치지 않는다.
    public abstract class Weapon : MonoBehaviour
    {
        // 하위 클래스는 읽기만 한다. 값은 Init으로만 들어온다.
        protected WeaponRuntimeStats Stats { get; private set; }
        protected EnemyRegistry Registry { get; private set; }

        // Bootstrap(나중에는 무기 인벤토리)이 호출한다.
        // registry를 인스펙터가 아니라 여기서 받는 이유: 프리팹으로 만든 무기는 씬 오브젝트를 인스펙터로 참조할 수 없다.
        public void Init(WeaponRuntimeStats stats, EnemyRegistry registry)
        {
            Stats = stats;
            Registry = registry;
        }

        // 하위 클래스가 Start를 덮어쓰면 base.Start()를 불러야 이 검사가 돈다.
        protected virtual void Start()
        {
            if (Stats == null || Registry == null)
            {
                Debug.LogError($"{GetType().Name}: Init을 받지 못했습니다. 씬의 Bootstrap 연결을 확인하세요.", this);
                // 꺼 두면 Update가 돌지 않아 매 프레임 NullReference가 나지 않는다.
                enabled = false;
            }
        }
    }
}
