using System.Collections.Generic;
using FiveMinuteHero.Enemies;
using FiveMinuteHero.Player;
using UnityEngine;

namespace FiveMinuteHero.Pickups
{
    // 적이 죽은 자리에 경험치 젬을 떨어뜨리고, 바닥에 남은 젬 목록을 들고 있다(자석이 한 번에 끌어올 때 쓴다).
    // 받는 쪽마다 다른 창구를 연다: 자석에는 IGemAttractor, 젬에는 IGemOwner.
    public class GemDropper : MonoBehaviour, IGemAttractor, IGemOwner
    {
        [SerializeField] private ExpGem _gemPrefab;
        [SerializeField] private PlayerExperience _collector;
        [SerializeField] private EnemyRegistry _registry;

        // 바닥에 남은 젬. 자석이 씬 전체를 검색하지 않고 이 목록만 돈다.
        // HashSet인 이유: 젬이 먹힐 때마다 빼는데 List.Remove는 앞에서부터 찾아 O(n)이고, HashSet은 O(1)이다. 순서는 필요 없다.
        // foreach는 struct 열거자라 순회할 때 할당이 생기지 않는다.
        private readonly HashSet<ExpGem> _gems = new HashSet<ExpGem>();

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
            gem.Init(enemy.ExpReward, _collector, this);
            _gems.Add(gem);
        }

        // 읽기와 바꾸기를 나눈다(#22·#32와 같은 원리). 여기서는 젬마다 "끌려가기 시작" 표시만 하고 목록은 건드리지 않는다.
        // 먹고 목록에서 빠지는 일은 각 젬이 자기 Update에서 한다. 순회 중에 HashSet이 바뀌면 InvalidOperationException이 난다.
        // 이 시점에 바닥에 있는 젬만 끌려온다. 이후에 떨어지는 젬은 평소처럼 흡수 범위로 끌린다.
        public void AttractAllGems()
        {
            foreach (ExpGem gem in _gems)
            {
                gem.Attract();
            }
        }

        // 젬이 먹히거나 파괴될 때 스스로 부른다. 이미 빠진 젬이면 아무 일도 하지 않으므로 두 번 불려도 안전하다.
        public void Unregister(ExpGem gem)
        {
            _gems.Remove(gem);
        }
    }
}
