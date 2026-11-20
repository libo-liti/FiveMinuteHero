using UnityEngine;

namespace FiveMinuteHero.Items
{
    // 바닥에 떨어진 아이템 하나. 무엇을 하는지는 모르고(ItemData에 맡김), 플레이어가 줍는 거리 안에 오면 수집 창구에 넘긴다.
    // 젬과 달리 끌려오지 않는다. 플레이어가 직접 가서 먹어야 해서 "지금 먹을지, 남겨 둘지"를 고를 수 있다.
    // 어디서 생겼는지(적 처치, 맵 상자)도 모른다. 만든 쪽이 Init만 불러 주면 된다.
    public class ItemPickup : MonoBehaviour
    {
        // 프리팹에서 연결한다. Awake의 GetComponent 대신 인스펙터 참조라서 Init이 Awake보다 먼저 와도 기댈 수 있다.
        [SerializeField] private SpriteRenderer _renderer;

        private ItemData _data;
        private PlayerItemCollector _collector;
        private Transform _collectorTransform; // 매 프레임 거리를 재므로 Init에서 한 번 받아 둔다
        private float _sqrPickupRadius;        // 제곱 거리로 비교해서 매번 제곱근을 계산하지 않는다

        // 만든 쪽(ItemDropper 등)이 생성 직후 호출한다.
        public void Init(ItemData data, PlayerItemCollector collector)
        {
            _data = data;
            _collector = collector;
            _collectorTransform = collector.transform;
            _sqrPickupRadius = data.PickupRadius * data.PickupRadius;
            // 종류마다 프리팹을 따로 두지 않고, 그림은 데이터에서 받아 하나의 프리팹을 같이 쓴다.
            _renderer.sprite = data.Icon;
        }

        private void Start()
        {
            if (_data == null || _collector == null)
            {
                Debug.LogError("ItemPickup: Init을 받지 못했습니다. 이 아이템을 만든 쪽을 확인하세요.", this);
                // 꺼 두면 Update가 돌지 않아 null 참조 에러가 매 프레임 쌓이지 않는다.
                enabled = false;
            }
        }

        // perf-baseline 이후지만 아이템은 화면에 몇 개뿐이라 젬처럼 각자 거리를 잰다.
        private void Update()
        {
            Vector2 offset = (Vector2)_collectorTransform.position - (Vector2)transform.position;
            if (offset.sqrMagnitude > _sqrPickupRadius)
            {
                return;
            }

            // 판이 끝나 효과를 못 냈으면(false) 사라지지 않는다. 효과 없이 아이템만 없어지는 일을 막는다.
            if (_collector.TryUse(_data))
            {
                Destroy(gameObject);
            }
        }
    }
}
