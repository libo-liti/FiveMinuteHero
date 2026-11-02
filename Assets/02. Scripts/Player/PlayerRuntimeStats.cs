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
    }
}
