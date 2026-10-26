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
        private bool _isAttracted;

        // GemDropper가 생성 직후 호출한다.
        public void Init(int amount, PlayerExperience collector)
        {
            _amount = amount;
            _collector = collector;
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
                Destroy(gameObject);
                return;
            }

            transform.position = Vector2.MoveTowards(position, target, _attractSpeed * Time.deltaTime);
        }
    }
}
