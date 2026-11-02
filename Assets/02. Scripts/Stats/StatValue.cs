using UnityEngine;

namespace FiveMinuteHero.Stats
{
    // 기본값에 보너스를 더해 최종 수치를 계산한다. 어떤 스탯인지, 보너스를 누가 주는지는 모른다.
    // 최종값 = (기본 + 고정 보너스 합) × (1 + % 보너스 합). %끼리는 더하므로 얻는 순서와 상관없이 결과가 같다.
    public class StatValue
    {
        private readonly float _baseValue;
        private readonly float _minValue; // 음수 % 보너스가 쌓여도 이 값 아래로는 내려가지 않는다(예: 쿨다운 0초 방지)
        private float _flatBonus;
        private float _percentBonus;

        public StatValue(float baseValue, float minValue)
        {
            _baseValue = baseValue;
            _minValue = minValue;
        }

        public float Value => Mathf.Max((_baseValue + _flatBonus) * (1f + _percentBonus), _minValue);

        public void AddFlat(float amount)
        {
            _flatBonus += amount;
        }

        // 0.1 = +10%. 공식에 그대로 넣을 수 있게 소수로 받는다.
        public void AddPercent(float amount)
        {
            _percentBonus += amount;
        }

        // 강화 데이터가 고른 방식대로 더한다. 런타임 스탯마다 "Flat이면 AddFlat, 아니면 AddPercent"를 반복하지 않으려고 둔다.
        public void Add(StatModifierType type, float amount)
        {
            switch (type)
            {
                case StatModifierType.Flat:
                    AddFlat(amount);
                    break;
                case StatModifierType.Percent:
                    AddPercent(amount);
                    break;
                default:
                    Debug.LogError($"StatValue: 처리하지 않는 보너스 방식입니다: {type}");
                    break;
            }
        }
    }
}
