using System.Collections.Generic;
using FiveMinuteHero.Enemies;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 이번 판에 가진 무기 목록이자, 무기를 만들고 주입하는 유일한 곳. Bootstrap이 만들고, UpgradeContext에는 IWeaponInventory로 넘긴다.
    // 시작 무기도, 판 중간에 얻는 무기도 Add 하나를 지나므로 생성 방식이 한 곳에만 있다.
    // MonoBehaviour가 아니라서 씬에 둘 필요가 없고, 필요한 것은 생성자로 받는다.
    public class WeaponInventory : IWeaponInventory
    {
        private readonly Transform _mount;          // 무기를 붙일 자리(플레이어 몸 높이). 무기가 플레이어를 따라다닌다
        private readonly EnemyRegistry _registry;   // 무기 Init에 넘긴다. 프리팹은 씬 오브젝트를 직접 참조할 수 없다

        // WeaponData SO 에셋을 키로 쓴다. 무기마다 에셋이 하나뿐이라 참조 비교로 충분하다.
        private readonly Dictionary<WeaponData, WeaponRuntimeStats> _weapons = new Dictionary<WeaponData, WeaponRuntimeStats>();

        public WeaponInventory(Transform mount, EnemyRegistry registry)
        {
            _mount = mount;
            _registry = registry;
        }

        // 무기 하나를 얻는다: 런타임 스탯 생성 → 프리팹 생성 → 주입 → 목록에 저장.
        // 무기 종류는 몰라도 된다. 무엇을 만들지는 data가, 어떻게 싸울지는 프리팹의 Weapon 하위 클래스가 정한다.
        public void Add(WeaponData data)
        {
            // Has의 ContainsKey는 null 키에 예외를 던지므로 가장 먼저 거른다. 빈 칸 하나가 Bootstrap.Awake 전체를 멈추면 안 된다.
            if (data == null)
            {
                Debug.LogError("WeaponInventory: 추가할 무기 데이터가 비어 있습니다. 시작 무기 목록이나 강화 에셋의 빈 칸을 확인하세요.");
                return;
            }

            if (Has(data))
            {
                Debug.LogError($"WeaponInventory: 이미 가진 무기입니다: {data.name}", data);
                return;
            }

            if (data.WeaponPrefab == null)
            {
                Debug.LogError($"WeaponInventory: 무기 프리팹이 비어 있습니다: {data.name}", data);
                return;
            }

            WeaponRuntimeStats stats = data.CreateRuntimeStats();
            // 부모를 지정하면 프리팹의 로컬 위치(0,0,0) 그대로 mount 위치에 붙는다.
            Weapon weapon = Object.Instantiate(data.WeaponPrefab, _mount);
            weapon.Init(stats, _registry);
            _weapons.Add(data, stats);
        }

        public bool Has(WeaponData data)
        {
            return _weapons.ContainsKey(data);
        }

        public WeaponRuntimeStats GetStats(WeaponData data)
        {
            return _weapons.TryGetValue(data, out WeaponRuntimeStats stats) ? stats : null;
        }
    }
}
