using FiveMinuteHero.Enemies;
using FiveMinuteHero.Player;
using UnityEngine;

namespace FiveMinuteHero.Pickups
{
    // 적이 죽은 자리에 경험치 젬을 떨어뜨린다.
    public class GemDropper : MonoBehaviour
    {
        [SerializeField] private ExpGem _gemPrefab;
        [SerializeField] private PlayerExperience _collector;
        [SerializeField] private EnemyRegistry _registry;

        // 바닥에 남아 있는 젬 수. 젬은 이 오브젝트 아래에만 생성되므로 자식 수가 곧 젬 수다(벤치마크 결과에 남긴다).
        // Destroy는 프레임 끝에 처리되어, 이번 프레임에 먹힌 젬 몇 개가 함께 세질 수 있다. 평균에는 영향이 거의 없다.
        public int ActiveGemCount => transform.childCount;

        private void OnEnable()
        {
            _registry.Registered += OnEnemyRegistered;
        }

        private void OnDisable()
        {
            _registry.Registered -= OnEnemyRegistered;
        }

        private void OnEnemyRegistered(Enemy enemy)
        {
            // 적이 파괴되면 이 구독도 적과 함께 사라지므로 따로 해제하지 않는다.
            enemy.Died += OnEnemyDied;
        }

        private void OnEnemyDied(Enemy enemy)
        {
            if (enemy.ExpReward <= 0)
            {
                return;
            }

            // Died는 Destroy 전에 불리므로 적의 위치를 아직 읽을 수 있다. 젬은 이 오브젝트 아래에 모아 하이어라키를 정리한다.
            ExpGem gem = Instantiate(_gemPrefab, enemy.transform.position, Quaternion.identity, transform);
            gem.Init(enemy.ExpReward, _collector);
        }
    }
}
