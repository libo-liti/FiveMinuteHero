using System.Collections.Generic;
using FiveMinuteHero.Enemies;
using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 연쇄 번개가 맞힌 적들을 잇는 선. 표시 전용이라 피해 판정에는 관여하지 않는다.
    // 공격마다 새로 만들지 않고 LineRenderer 하나를 다시 쓴다. 생성·파괴와 GC가 없고, 그리기는 선 하나(배치 1개)뿐이다.
    public class ChainLightningEffect : MonoBehaviour
    {
        // 재생 시간이 0이면 진행도 계산이 0으로 나누기가 된다.
        private const float MinDuration = 0.01f;

        [SerializeField] private LineRenderer _line;
        [SerializeField, Min(MinDuration)] private float _duration = 0.15f; // 선이 보였다가 사라지기까지의 시간(초)

        // 프리팹에 지정한 선 색. 여기서부터 투명도만 0까지 줄인다.
        private Color _startColor;
        private Color _endColor;
        private float _elapsed;

        private void Awake()
        {
            _startColor = _line.startColor;
            _endColor = _line.endColor;

            // 첫 공격 전에는 아무것도 그리지 않고, Update도 돌지 않게 꺼 둔다.
            _line.enabled = false;
            enabled = false;
        }

        // 무기가 피해를 주기 전에 호출한다. origin(무기 위치)에서 시작해 맞은 순서대로 적을 잇는다.
        // 적 참조를 들고 있지 않고 지금 위치를 복사한다. 죽은 적이 풀에서 다른 자리로 재사용돼도 선은 맞은 자리에 남는다.
        public void Show(Vector2 origin, IReadOnlyList<Enemy> targets)
        {
            // SetPosition을 한 점씩 불러서 배열을 새로 만들지 않는다.
            _line.positionCount = targets.Count + 1;
            _line.SetPosition(0, origin);
            for (int i = 0; i < targets.Count; i++)
            {
                _line.SetPosition(i + 1, targets[i].AimPoint);
            }

            // 이전 선이 아직 사라지는 중이어도 처음부터 다시 보인다. 쿨다운이 재생 시간보다 짧아도 선이 겹치거나 남지 않는다.
            _elapsed = 0f;
            SetAlpha(1f);
            _line.enabled = true;
            enabled = true;
        }

        private void Update()
        {
            // deltaTime을 쓰므로 레벨업 정지 중에는 선도 그 상태로 멈춘다.
            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _duration);
            SetAlpha(1f - progress);

            if (progress >= 1f)
            {
                // 오브젝트를 끄지 않고 컴포넌트만 끈다. 다음 Show에서 켜기만 하면 되고, 끄는 동안 Update 비용도 없다.
                _line.enabled = false;
                enabled = false;
            }
        }

        // 프리팹 색의 투명도에 비율을 곱한다. Color는 구조체라 할당이 없다.
        private void SetAlpha(float ratio)
        {
            Color start = _startColor;
            start.a *= ratio;
            Color end = _endColor;
            end.a *= ratio;
            _line.startColor = start;
            _line.endColor = end;
        }
    }
}
