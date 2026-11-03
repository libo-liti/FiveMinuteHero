using System.Collections.Generic;
using FiveMinuteHero.Stats;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 플레이 중의 무기 수치. WeaponData SO를 기본값으로 쓰고, 강화 보너스는 여기에만 쌓는다(이유는 PlayerRuntimeStats와 같다).
    // 어떤 무기인지 모른다. 어떤 스탯을 가질지는 WeaponData.CreateRuntimeStats가 Define으로 정해 준다.
    public class WeaponRuntimeStats
    {
        // 이 무기가 가진 스탯만 들어 있다. 없는 스탯을 읽거나 강화하면 잘못된 조합이므로 에러로 알린다.
        private readonly Dictionary<WeaponStatType, StatValue> _stats = new Dictionary<WeaponStatType, StatValue>();
        // 스탯 등록이 끝났는지. 잠근 뒤에는 스탯 구성이 바뀌지 않으므로 "없는 스탯은 강화 불가" 검사를 믿을 수 있다.
        private bool _isLocked;

        // 피해량은 모든 무기에 있고 정수라서 따로 꺼내 둔다. % 보너스로 생긴 소수는 반올림한다.
        public int Damage => Mathf.RoundToInt(Get(WeaponStatType.Damage));

        // 이 무기가 가질 스탯 하나를 등록한다. WeaponData.CreateRuntimeStats 안에서, Lock 전에만 부른다.
        // 같은 스탯을 두 번 넣으면 Add가 예외를 던져 실수를 바로 알 수 있다.
        public void Define(WeaponStatType stat, float baseValue, float minValue)
        {
            // 판 중간에 강화·무기 코드가 스탯을 새로 끼워 넣으면 AddBonus의 조합 검사가 뚫린다. 그래서 막는다.
            if (_isLocked)
            {
                Debug.LogError($"WeaponRuntimeStats: 등록이 끝난 뒤에는 스탯을 추가할 수 없습니다: {stat}");
                return;
            }

            _stats.Add(stat, new StatValue(baseValue, minValue));
        }

        // 스탯 등록을 끝낸다. WeaponData.CreateRuntimeStats가 반환 직전에 한 번 부른다.
        public void Lock()
        {
            _isLocked = true;
        }

        // 현재 값(기본 + 보너스). 이 무기에 없는 스탯이면 알리고 0을 돌려준다.
        public float Get(WeaponStatType stat)
        {
            if (!_stats.TryGetValue(stat, out StatValue value))
            {
                Debug.LogError($"WeaponRuntimeStats: 이 무기에 없는 스탯을 읽으려 했습니다: {stat}");
                return 0f;
            }

            return value.Value;
        }

        // 강화 하나를 이 객체에 쌓는다. 무기는 발사할 때마다 값을 다시 읽으므로 이벤트로 알리지 않는다.
        public void AddBonus(WeaponStatType stat, StatModifierType type, float amount)
        {
            // 예: 오브 무기에 투사체 속도 강화를 연결한 에셋. switch의 default 대신 사전에 있는지로 거른다.
            if (!_stats.TryGetValue(stat, out StatValue value))
            {
                Debug.LogError($"WeaponRuntimeStats: 이 무기에 없는 스탯은 강화할 수 없습니다: {stat}");
                return;
            }

            value.Add(type, amount);
        }
    }
}
