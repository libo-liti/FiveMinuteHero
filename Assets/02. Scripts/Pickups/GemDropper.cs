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

        // 적 하나하나가 아니라 오래 사는 레지스트리에 한 번만 붙는다. 적이 몇 번 재사용돼도 처치 1번에 젬 1개다.
        private void OnEnable()
        {
            _registry.EnemyDied += OnEnemyDied;
        }

        private void OnDisable()
        {
            _registry.EnemyDied -= OnEnemyDied;
        }

        private void OnEnemyDied(Enemy enemy)
        {
            if (enemy.ExpReward <= 0)
            {
                return;
            }

            // 적의 풀 반납은 프레임 끝으로 미뤄지므로 여기서는 적의 위치를 아직 읽을 수 있다. 젬은 이 오브젝트 아래에 모아 하이어라키를 정리한다.
            ExpGem gem = Instantiate(_gemPrefab, enemy.transform.position, Quaternion.identity, transform);
            gem.Init(enemy.ExpReward, _collector);
        }
    }
}
