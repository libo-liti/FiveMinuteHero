using UnityEngine;

namespace FiveMinuteHero.Player
{
    // 플레이어 수치 데이터. 코드 수정 없이 인스펙터에서 밸런스를 바꾸기 위해 SO로 분리한다.
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "FiveMinuteHero/Player Stats")]
    public class PlayerStats : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _moveSpeed = 3f; // 초당 이동 거리(유닛)

        public float MoveSpeed => _moveSpeed;
    }
}
