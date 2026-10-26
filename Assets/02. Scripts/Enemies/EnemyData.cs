using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 적 종류별 수치 데이터. 박쥐·슬라임처럼 종류마다 SO 에셋을 하나씩 만들고, 프리팹이 참조한다.
    [CreateAssetMenu(fileName = "EnemyData", menuName = "FiveMinuteHero/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _moveSpeed = 1.5f; // 초당 이동 거리(유닛)
        [SerializeField, Min(1)] private int _maxHealth = 10;
        [SerializeField, Min(0)] private int _contactDamage = 1;   // 플레이어에 닿았을 때 주는 피해
        [SerializeField, Min(0)] private int _expReward = 1;       // 죽을 때 떨어뜨리는 젬의 경험치. 0이면 젬 없음

        public float MoveSpeed => _moveSpeed;
        public int MaxHealth => _maxHealth;
        public int ContactDamage => _contactDamage;
        public int ExpReward => _expReward;
    }
}
