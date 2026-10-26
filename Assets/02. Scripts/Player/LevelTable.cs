using UnityEngine;

namespace FiveMinuteHero.Player
{
    // 레벨별 필요 경험치 표. 레벨마다 값을 직접 넣어 특정 구간만 따로 조절할 수 있게 한다.
    [CreateAssetMenu(fileName = "LevelTable", menuName = "FiveMinuteHero/Level Table")]
    public class LevelTable : ScriptableObject
    {
        // [0]은 1→2레벨, [1]은 2→3레벨에 필요한 경험치. 표 끝을 넘는 레벨은 마지막 값을 계속 쓴다.
        [SerializeField, Min(1)] private int[] _expToNextLevel = { 5, 8, 12, 16, 20 };

        // level은 1부터 시작한다.
        public int GetRequiredExp(int level)
        {
            int index = Mathf.Clamp(level - 1, 0, _expToNextLevel.Length - 1);
            return _expToNextLevel[index];
        }
    }
}
