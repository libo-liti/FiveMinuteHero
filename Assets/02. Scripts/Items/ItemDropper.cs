using FiveMinuteHero.Enemies;
using UnityEngine;

namespace FiveMinuteHero.Items
{
    // 적이 죽을 때 확률표를 굴려 바닥 아이템을 떨어뜨린다. 어떤 아이템이 무슨 효과인지는 모른다(표와 ItemData에 맡김).
    // Main 씬에만 둔다(GameCore 밖). Benchmark에는 없어서 측정 조건(적 처치당 생기는 오브젝트, 난수 순서)이 바뀌지 않는다.
    // GemDropper와 따로 EnemyDied를 구독한다. 한 적이 젬과 아이템을 함께 떨어뜨릴 수 있고, 둘은 서로를 모른다.
    public class ItemDropper : MonoBehaviour
    {
        [SerializeField] private ItemDropTable _table;
        [SerializeField] private ItemPickup _pickupPrefab;
        [SerializeField] private PlayerItemCollector _collector; // 생성한 아이템에 넘겨 줄 수집 창구
        [SerializeField] private EnemyRegistry _registry;

        // 드롭 전용 난수. 스포너는 스폰 전용 System.Random을, 무기·레벨업은 UnityEngine.Random을 쓴다.
        // 같은 난수를 같이 쓰면 처치가 많을수록 다른 시스템의 난수 순서가 밀리므로(고정 시드 재현이 깨진다) 따로 둔다.
        // 시드 없이 만들어 매판 다르다. using System을 쓰면 UnityEngine.Random과 이름이 겹쳐 전체 이름으로 적는다.
        private readonly System.Random _random = new System.Random();

        // 적 하나하나가 아니라 오래 사는 레지스트리에 한 번만 붙는다(GemDropper와 같은 방식). 적 쪽 코드는 이 클래스를 모른다.
        private void OnEnable()
        {
            _registry.EnemyDied += OnEnemyDied;
        }

        private void OnDisable()
        {
            _registry.EnemyDied -= OnEnemyDied;
        }

        // 데이터 실수는 조용히 넘기지 않고 판 시작 때 한 번 알린다.
        private void Start()
        {
            if (_table == null || _pickupPrefab == null || _collector == null)
            {
                Debug.LogError("ItemDropper: 확률표·아이템 프리팹·수집 창구 중 비어 있는 칸이 있습니다.", this);
                // 꺼 두면 OnDisable에서 구독이 풀려 적이 죽을 때마다 null 참조 에러가 나지 않는다.
                enabled = false;
                return;
            }

            if (_table.TotalChance > 1f)
            {
                Debug.LogError($"ItemDropper: {_table.name}의 확률 합이 1을 넘습니다({_table.TotalChance}). 뒤쪽 아이템이 적힌 확률보다 적게 나옵니다.", this);
            }
        }

        private void OnEnemyDied(Enemy enemy)
        {
            // [0, 1) 난수 하나로 표 전체를 한 번 굴린다.
            ItemData item = _table.Pick(_random.NextDouble());
            if (item == null)
            {
                return;
            }

            // 적의 풀 반납은 프레임 끝으로 미뤄지므로 여기서는 적의 위치를 아직 읽을 수 있다. 아이템은 이 오브젝트 아래에 모은다.
            ItemPickup pickup = Instantiate(_pickupPrefab, enemy.transform.position, Quaternion.identity, transform);
            pickup.Init(item, _collector);
        }
    }
}
