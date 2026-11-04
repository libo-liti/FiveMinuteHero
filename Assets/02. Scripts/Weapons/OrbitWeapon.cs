using System.Collections.Generic;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 오브들을 원 위에 같은 간격으로 놓고 플레이어 주위로 돌린다. 쿨다운도 조준도 없는 무기다.
    // 오브는 이 오브젝트의 자식이라서, 이 오브젝트 하나만 돌리면 오브 전체가 함께 돈다.
    public class OrbitWeapon : Weapon
    {
        private const float FullCircleDegrees = 360f;

        [SerializeField] private Orb _orbPrefab;

        private readonly List<Orb> _orbs = new List<Orb>();

        // Init이 Instantiate 다음에 오므로 OnEnable에서는 Stats가 아직 없다. 그래서 Start에서 구독한다.
        protected override void Start()
        {
            base.Start();
            // Init을 못 받아 부모가 꺼 둔 경우. 이대로 진행하면 Stats가 null이다.
            if (!enabled)
            {
                return;
            }

            Stats.Changed += OnStatChanged;
            ArrangeOrbs();
        }

        private void OnDestroy()
        {
            // 스탯 객체는 인벤토리에 남을 수 있다. 파괴된 무기가 구독자로 남지 않게 끊는다.
            if (Stats != null)
            {
                Stats.Changed -= OnStatChanged;
            }
        }

        private void Update()
        {
            // timeScale이 0이면 deltaTime도 0이라서, 레벨업 창이 떠 있는 동안 회전도 멈춘다.
            transform.Rotate(0f, 0f, Stats.Get(WeaponStatType.RotationSpeed) * Time.deltaTime);
        }

        private void OnStatChanged(WeaponStatType stat)
        {
            // 회전 속도·피해량 등은 쓸 때마다 다시 읽으므로 배치를 건드릴 필요가 없다.
            if (stat == WeaponStatType.OrbCount || stat == WeaponStatType.OrbitRadius)
            {
                ArrangeOrbs();
            }
        }

        // 오브 수를 스탯에 맞추고 원 위에 다시 놓는다.
        private void ArrangeOrbs()
        {
            // 강화로 소수가 생길 수 있어서 가장 가까운 정수로 맞춘다.
            int count = Mathf.RoundToInt(Stats.Get(WeaponStatType.OrbCount));

            // 전부 지우고 새로 만들지 않는다. 이미 있는 오브를 살려 두면 오브별 상태가 유지된다.
            while (_orbs.Count < count)
            {
                Orb orb = Instantiate(_orbPrefab, transform);
                orb.Init(Stats);
                _orbs.Add(orb);
            }

            while (_orbs.Count > count)
            {
                int last = _orbs.Count - 1;
                Destroy(_orbs[last].gameObject);
                _orbs.RemoveAt(last);
            }

            float radius = Stats.Get(WeaponStatType.OrbitRadius);
            float stepDegrees = FullCircleDegrees / count;
            for (int i = 0; i < count; i++)
            {
                float radians = stepDegrees * i * Mathf.Deg2Rad;
                // 부모가 돌고 있어도 로컬 좌표라서 현재 회전 각도를 따로 계산하지 않아도 된다.
                _orbs[i].transform.localPosition = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * radius;
            }
        }
    }
}
