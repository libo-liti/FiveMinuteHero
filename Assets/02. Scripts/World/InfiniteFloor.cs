using UnityEngine;

namespace FiveMinuteHero.World
{
    // Tiled SpriteRenderer 하나로 무한 바닥을 만든다.
    // 카메라 화면을 덮는 크기로 맞추고, 타일 한 칸 단위로 스냅해 카메라를 따라가서 무늬가 움직이지 않게 보인다.
    [RequireComponent(typeof(SpriteRenderer))]
    public class InfiniteFloor : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField, Min(1)] private int _marginTiles = 2; // 화면 바깥 한쪽에 더 깔아 둘 칸 수

        private SpriteRenderer _renderer;
        private float _tileSize;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _tileSize = _renderer.sprite.bounds.size.x; // PPU 16, 16px 타일이면 1유닛
        }

        private void LateUpdate()
        {
            UpdateSize();

            // 반올림으로 타일 한 칸 단위로만 이동시킨다. 한 칸씩 점프해도 같은 무늬라 눈에 띄지 않는다.
            Vector3 cameraPosition = _camera.transform.position;
            transform.position = new Vector3(
                Mathf.Round(cameraPosition.x / _tileSize) * _tileSize,
                Mathf.Round(cameraPosition.y / _tileSize) * _tileSize,
                transform.position.z);
        }

        // 기기 화면비마다 보이는 영역이 달라서(Pixel Perfect Camera) 카메라 기준으로 크기를 계산한다.
        private void UpdateSize()
        {
            float viewHeight = _camera.orthographicSize * 2f;
            float viewWidth = viewHeight * _camera.aspect;
            Vector2 size = new Vector2(ToEvenTileCount(viewWidth), ToEvenTileCount(viewHeight)) * _tileSize;

            // size를 바꾸면 메시가 다시 만들어지므로 값이 달라졌을 때만 넣는다.
            if (_renderer.size != size)
            {
                _renderer.size = size;
            }
        }

        // 칸 수를 짝수로 맞춰 타일 경계가 항상 격자 위에 오게 한다(홀수면 반 칸 어긋남).
        private int ToEvenTileCount(float length)
        {
            int count = Mathf.CeilToInt(length / _tileSize) + _marginTiles * 2;
            return count % 2 == 0 ? count : count + 1;
        }
    }
}
