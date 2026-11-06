using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 맞은 적을 잠깐 하얗게 번쩍인다. 표시만 맡으며 전투 결과에는 관여하지 않는다.
    //
    // 색 약속(SharedSpriteLit 셰이더): SpriteRenderer.color의 R은 색이 아니라 "1 - 깜빡임 세기"다.
    // 그래서 이 컴포넌트를 쓰는 적에는 색 틴트를 쓸 수 없다. 재질이나 MaterialPropertyBlock을 건드리지 않아 배칭이 유지된다.
    public class EnemyHitFlash : MonoBehaviour
    {
        // 재생 시간이 0이면 세기 계산이 0으로 나누기가 된다.
        private const float MinDuration = 0.01f;
        private const float FullStrength = 1f;

        [SerializeField] private EnemyHealth _health;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField, Min(MinDuration)] private float _duration = 0.1f; // 흰색에서 원래 색으로 돌아오는 시간(초)

        private float _remaining; // 깜빡임이 끝날 때까지 남은 시간(초)

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
        }

        private void OnDamaged()
        {
            // 연속으로 맞으면 처음부터 다시 번쩍인다.
            _remaining = _duration;
            // 피격은 Update 이후에도 올 수 있어서, 타이머를 기다리지 않고 이번 프레임에 바로 하얗게 만든다.
            ApplyStrength(FullStrength);
        }

        private void Update()
        {
            if (_remaining <= 0f)
            {
                return;
            }

            // deltaTime을 쓰므로 레벨업 정지 중에는 깜빡임도 함께 멈춘다.
            _remaining -= Time.deltaTime;
            // 음수가 되는 마지막 프레임에 Clamp로 세기 0을 넣어 원래 색으로 정확히 돌려놓는다.
            ApplyStrength(Mathf.Clamp01(_remaining / _duration));
        }

        private void ApplyStrength(float strength)
        {
            Color color = _renderer.color;
            color.r = FullStrength - strength;
            _renderer.color = color;
        }
    }
}
