using System;
using UnityEngine;

namespace FiveMinuteHero.Player
{
    // 플레이어 경험치와 레벨. 젬이 AddExp로 넣어 주고, 변화는 이벤트로 알린다.
    public class PlayerExperience : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;
        [SerializeField] private LevelTable _levelTable;

        private int _level = 1;
        private int _currentExp; // 현재 레벨 안에서 쌓인 경험치. 레벨업하면 필요량만큼 빠진다.

        // (현재 경험치, 이번 레벨 필요 경험치). 경험치 바 UI가 구독한다.
        public event Action<int, int> ExpChanged;
        // 오른 뒤의 레벨. 다음 단계의 레벨업 선택 UI가 구독한다.
        public event Action<int> LeveledUp;

        public int Level => _level;
        public int CurrentExp => _currentExp;
        public int RequiredExp => _levelTable.GetRequiredExp(_level);
        // 젬이 끌려오기 시작할 거리. 젬은 PlayerStats를 몰라도 되게 여기서 넘겨준다.
        public float PickupRange => _stats.PickupRange;

        public void AddExp(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _currentExp += amount;

            // 큰 젬 하나로 필요량을 두 번 넘길 수 있어서 if가 아니라 while로 처리한다.
            while (_currentExp >= RequiredExp)
            {
                _currentExp -= RequiredExp; // 넘친 경험치는 다음 레벨로 이월
                _level++;
                Debug.Log($"[PlayerExperience] 레벨업 → Lv.{_level}");
                LeveledUp?.Invoke(_level);
            }

            ExpChanged?.Invoke(_currentExp, RequiredExp);
        }
    }
}
