using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 폭발 범위를 보여 주는 표시 전용 오브젝트. 앞부분에 반경까지 커지고, 뒷부분에 그 크기로 투명해진 뒤 사라진다.
    // 피해 판정은 ExplosionWeapon이 끝낸 뒤라서, 외형을 바꿔도 전투 결과는 달라지지 않는다.
    public class ExplosionEffect : MonoBehaviour
    {
        // 재생 시간 중 커지는 데 쓰는 비율. 나머지는 최대 크기로 투명해진다. 최대 크기가 보여야 판정 범위와 비교할 수 있다.
        private const float GrowPortion = 0.5f;
        // 재생 시간이 0이면 진행도 계산이 0으로 나누기가 된다.
        private const float MinDuration = 0.01f;

        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField, Min(MinDuration)] private float _duration = 0.3f; // 전체 재생 시간(초)

        private float _fullScale;   // 스프라이트 지름이 폭발 지름과 같아지는 스케일
        private float _startAlpha;  // 프리팹에 지정한 색의 투명도. 여기서부터 0까지 줄인다
        private float _elapsed;

        // 무기가 생성 직후 호출한다. 표시할 범위만 받고, 그 뒤로는 스스로 재생하고 사라진다.
        public void Play(float radius)
        {
            // sprite.bounds는 스케일 1일 때의 유닛 크기다. 스프라이트를 바꿔도 지름이 그대로 맞는다.
            _fullScale = radius * 2f / _renderer.sprite.bounds.size.x;
            _startAlpha = _renderer.color.a;
            // 첫 Update는 다음 프레임이라서, 이대로 두면 한 프레임 동안 프리팹 크기로 보인다.
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            // deltaTime을 쓰므로 레벨업 정지 중에는 이펙트도 함께 멈춘다.
            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _duration);

            float growProgress = Mathf.Clamp01(progress / GrowPortion);
            transform.localScale = Vector3.one * (_fullScale * growProgress);

            float fadeProgress = Mathf.Clamp01((progress - GrowPortion) / (1f - GrowPortion));
            Color color = _renderer.color;
            color.a = _startAlpha * (1f - fadeProgress);
            _renderer.color = color;

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
