using UnityEngine;

namespace FiveMinuteHero.World
{
    // Tiled SpriteRenderer 하나로 무한 바닥을 만든다.
    // 카메라 화면을 덮는 크기로 맞추고, 무늬 한 장 단위로 스냅해 카메라를 따라가서 무늬가 움직이지 않게 보인다.
    [RequireComponent(typeof(SpriteRenderer))]
    public class InfiniteFloor : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField, Min(1)] private int _marginPatterns = 1; // 화면 바깥 한쪽에 더 깔아 둘 무늬 장 수

        private SpriteRenderer _renderer;
        private float _patternSize;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _patternSize = _renderer.sprite.bounds.size.x; // PPU 16, 256px 무늬면 16유닛
        }

        private void LateUpdate()
        {
            UpdateSize();

            // 반올림으로 무늬 한 장 단위로만 이동시킨다. 한 장씩 점프하면 같은 무늬가 같은 자리에 와서 눈에 띄지 않는다.
            // 한 칸(16px)씩 옮기면 무늬 시작점이 같이 밀려 화면 전체 무늬가 튄다.
            Vector3 cameraPosition = _camera.transform.position;
            transform.position = new Vector3(
                Mathf.Round(cameraPosition.x / _patternSize) * _patternSize,
                Mathf.Round(cameraPosition.y / _patternSize) * _patternSize,
                transform.position.z);
        }

        // 기기 화면비마다 보이는 영역이 달라서(Pixel Perfect Camera) 카메라 기준으로 크기를 계산한다.
        private void UpdateSize()
        {
            float viewHeight = _camera.orthographicSize * 2f;
            float viewWidth = viewHeight * _camera.aspect;
            Vector2 size = new Vector2(ToEvenPatternCount(viewWidth), ToEvenPatternCount(viewHeight)) * _patternSize;

            // size를 바꾸면 메시가 다시 만들어지므로 값이 달라졌을 때만 넣는다.
            if (_renderer.size != size)
            {
                _renderer.size = size;
            }
        }

        // 장 수를 짝수로 맞춰 무늬 경계가 항상 격자 위에 오게 한다(홀수면 반 장 어긋남).
        // 스냅 사이에서 카메라는 최대 반 장 어긋나므로, 양쪽 여백 1장이면 화면 가장자리가 비지 않는다.
        private int ToEvenPatternCount(float length)
        {
            int count = Mathf.CeilToInt(length / _patternSize) + _marginPatterns * 2;
            return count % 2 == 0 ? count : count + 1;
        }
    }
}
