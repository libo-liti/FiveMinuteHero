using UnityEngine;

namespace FiveMinuteHero.Items
{
    // 아이템 하나의 데이터와 효과. 무엇을 하는지는 하위 클래스가 Apply로 정한다(UpgradeData와 같은 방식).
    // 어디서 몇 % 확률로 나오는지는 여기 두지 않는다. 그건 드롭하는 쪽(ItemDropTable)의 데이터라서,
    // 적 처치든 맵 상자든 같은 아이템 에셋을 그대로 떨어뜨릴 수 있다. 드롭·줍기 코드는 하위 종류를 모른다.
    // 추상 클래스는 에셋으로 만들 수 없어서 [CreateAssetMenu]는 하위 클래스에 붙인다.
    public abstract class ItemData : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [Tooltip("플레이어가 이 거리 안에 들어오면 먹는다(월드 단위). 손가락 조작은 정확히 멈추기 어려워 젬 수집 거리(0.3)보다 넉넉히 둔다.")]
        [SerializeField, Min(0.1f)] private float _pickupRadius = 0.7f;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        // 줍는 거리는 바닥에 놓인 물건의 성질이라 아이템마다 둔다. 어디서 떨어졌든 같은 거리로 먹힌다.
        public float PickupRadius => _pickupRadius;

        // 먹었을 때 한 번 불린다. 판이 끝났는지 같은 검사는 부르는 쪽(PlayerItemCollector)이 맡고, 여기서는 효과만 낸다.
        public abstract void Apply(ItemContext context);
    }
}
