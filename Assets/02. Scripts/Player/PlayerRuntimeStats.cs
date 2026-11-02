using System;
using FiveMinuteHero.Stats;
using UnityEngine;

namespace FiveMinuteHero.Player
{
    // 플레이 중의 플레이어 수치. PlayerStats SO를 기본값으로 쓰고, 강화 보너스는 여기에만 쌓는다.
    // SO 원본을 바꾸면 에디터에서 에셋에 저장돼 밸런스 값이 오염되므로, 한 판 동안 바뀌는 값은 이 객체가 들고 있는다.
    public class PlayerRuntimeStats
    {
        // 최솟값은 PlayerStats의 [Min]과 맞춘다. 보너스가 없으면 SO 값이 그대로 나온다.
        private const float MinMoveSpeed = 0f;
        private const float MinMaxHealth = 1f;
        private const float MinPickupRange = 0f;

        private readonly StatValue _moveSpeed;
        private readonly StatValue _maxHealth;
        private readonly StatValue _pickupRange;
        private readonly float _invincibleDuration; // 강화 대상이 아니라서 SO 값을 그대로 쓴다

        // (이전 최대 체력, 새 최대 체력). 반올림한 정수 값이 실제로 바뀌었을 때만 발생한다. PlayerHealth가 구독한다.
        public event Action<int, int> MaxHealthChanged;

        public PlayerRuntimeStats(PlayerStats source)
        {
            _moveSpeed = new StatValue(source.MoveSpeed, MinMoveSpeed);
            _maxHealth = new StatValue(source.MaxHealth, MinMaxHealth);
            _pickupRange = new StatValue(source.PickupRange, MinPickupRange);
            _invincibleDuration = source.InvincibleDuration;
        }

        public float MoveSpeed => _moveSpeed.Value;
        // 체력은 정수라서 % 보너스로 생긴 소수는 반올림한다.
        public int MaxHealth => Mathf.RoundToInt(_maxHealth.Value);
        public float PickupRange => _pickupRange.Value;
        public float InvincibleDuration => _invincibleDuration;

        // 강화 하나를 이 객체에 쌓는다. 이동 속도·흡수 범위는 쓰는 쪽이 매번 값을 다시 읽어서 따로 알릴 필요가 없다.
        public void AddBonus(PlayerStatType stat, StatModifierType type, float amount)
        {
            switch (stat)
            {
                case PlayerStatType.MoveSpeed:
                    _moveSpeed.Add(type, amount);
                    break;
                case PlayerStatType.MaxHealth:
                    // 현재 체력은 PlayerHealth가 따로 들고 있어서, 최대치가 바뀌면 알려야 맞춰 줄 수 있다.
                    // 반올림 때문에 작은 % 보너스는 정수 값이 안 바뀔 수 있으니, 더하기 전 값과 비교해 바뀐 경우만 알린다.
                    int oldMaxHealth = MaxHealth;
                    _maxHealth.Add(type, amount);
                    if (oldMaxHealth != MaxHealth)
                    {
                        MaxHealthChanged?.Invoke(oldMaxHealth, MaxHealth);
                    }
                    break;
                case PlayerStatType.PickupRange:
                    _pickupRange.Add(type, amount);
                    break;
                default:
                    Debug.LogError($"PlayerRuntimeStats: 처리하지 않는 스탯입니다: {stat}");
                    break;
            }
        }
    }
}
