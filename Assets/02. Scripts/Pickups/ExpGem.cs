using FiveMinuteHero.Player;
using UnityEngine;

namespace FiveMinuteHero.Pickups
{
    // 바닥에 떨어진 경험치 젬. 플레이어가 흡수 범위에 들어오면 끌려가고, 닿으면 경험치를 주고 사라진다.
    public class ExpGem : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _attractSpeed = 8f;      // 끌려갈 때 초당 이동 거리. 플레이어 이동 속도보다 빨라야 따라잡는다
        [SerializeField, Min(0f)] private float _collectDistance = 0.3f; // 이 거리 안이면 먹힌 것으로 본다

        private int _amount;
        private PlayerExperience _collector;
        private IGemOwner _owner; // 이 젬을 목록에 넣은 쪽. 먹히거나 파괴될 때 목록에서 빼 달라고 알린다
        private bool _isAttracted;

        // GemDropper가 생성 직후 호출한다.
        public void Init(int amount, PlayerExperience collector, IGemOwner owner)
        {
            _amount = amount;
            _collector = collector;
            _owner = owner;
        }

        // 자석이 부른다. 표시만 하고 실제 이동·수집은 Update가 한다(부르는 쪽이 목록을 도는 중이라 여기서 목록을 바꾸면 안 된다).
        public void Attract()
        {
            _isAttracted = true;
        }

        // perf-baseline 전이라 젬마다 매 프레임 거리를 직접 잰다(O(n)).
        private void Update()
        {
            Vector2 position = transform.position;
            Vector2 target = _collector.transform.position;
            // 제곱 거리로 비교해서 매번 제곱근을 계산하지 않는다.
            float sqrDistance = (target - position).sqrMagnitude;

            if (!_isAttracted)
            {
                float range = _collector.PickupRange;
                if (sqrDistance > range * range)
                {
                    return;
                }

                // 한 번 끌리기 시작하면 플레이어가 범위 밖으로 달아나도 끝까지 따라간다.
                _isAttracted = true;
            }

            if (sqrDistance <= _collectDistance * _collectDistance)
            {
                _collector.AddExp(_amount);
                // Destroy는 프레임 끝에 처리되므로 지금 바로 목록에서 뺀다. 그래야 같은 프레임의 자석이 먹힌 젬을 다시 건드리지 않는다.
                _owner.Unregister(this);
                Destroy(gameObject);
                return;
            }

            transform.position = Vector2.MoveTowards(position, target, _attractSpeed * Time.deltaTime);
        }

        // 먹히지 않고 파괴되는 길(씬 재시작 등)에서도 목록에 남지 않게 하는 안전망. 먹힌 경우엔 이미 빠져 있어 아무 일도 없다.
        // _owner는 인터페이스라 null 검사가 C# 기본 비교다(Unity의 "파괴됨 = null" 판정을 거치지 않는다). 그래서 Init을 못 받은 경우만 걸러진다.
        // 씬을 내릴 때 주인이 먼저 파괴돼도 그대로 불리는데, Unregister는 HashSet만 만지고 Unity API를 쓰지 않아 안전하다.
        private void OnDestroy()
        {
            _owner?.Unregister(this);
        }
    }
}
