using UnityEngine;

namespace FiveMinuteHero.Player
{
    // 성능 측정용 입력. 고정 중심 주위를 반시계로 돌고, 적에게 밀려나도 같은 궤도로 돌아온다.
    public class OrbitBotMoveInput : MonoBehaviour, IMoveInput
    {
        // 이보다 중심에 가까우면 바깥 방향을 정할 수 없다고 본다(유닛).
        private const float MinDistanceFromCenter = 0.01f;

        [SerializeField] private Vector2 _center; // 궤도 중심(월드 좌표)
        [SerializeField, Min(1f)] private float _radius = 5f; // 궤도 반지름(유닛)
        // 궤도에서 벗어난 만큼 되돌아오는 세기. 크면 빨리 돌아오지만, 너무 크면 궤도 근처에서 흔들린다.
        [SerializeField, Min(0f)] private float _correctionStrength = 1f;

        // PlayerMovement가 매 프레임 읽는다. 읽는 순간의 위치로 계산하므로 캐시할 상태가 없다.
        public Vector2 Direction => CalculateDirection();

        private Vector2 CalculateDirection()
        {
            Vector2 centerToBot = (Vector2)transform.position - _center;
            float distance = centerToBot.magnitude;

            // 중심에서는 바깥 방향이 정해지지 않으므로, 아무 방향으로 나가서 궤도부터 찾는다.
            if (distance < MinDistanceFromCenter)
            {
                return Vector2.right;
            }

            Vector2 outward = centerToBot / distance;
            // 바깥 방향을 반시계로 90도 돌리면 원을 따라 도는 방향이 된다.
            Vector2 tangent = Vector2.Perpendicular(outward);
            // 접선만 따르면 밀릴 때마다 원이 흘러간다. 반지름과의 차이만큼 안팎으로 당겨 같은 원에 붙잡는다.
            Vector2 correction = outward * ((_radius - distance) * _correctionStrength);

            return (tangent + correction).normalized;
        }
    }
}
