using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 일정 간격으로 카메라 화면 바로 바깥에 적을 만들고, 추적 대상을 주입한다.
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _target;
        [SerializeField] private EnemyMovement[] _enemyPrefabs;
        [SerializeField, Min(0.01f)] private float _spawnInterval = 1f; // 초
        // 원점이 발이라 스프라이트가 위로 약 3유닛 솟는다. 아래쪽에서 나올 때도 안 보이도록 이만큼 띄운다.
        [SerializeField, Min(0f)] private float _spawnMargin = 3f;

        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < _spawnInterval)
            {
                return;
            }

            _timer -= _spawnInterval;
            Spawn();
        }

        private void Spawn()
        {
            EnemyMovement prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
            EnemyMovement enemy = Instantiate(prefab, GetSpawnPosition(), Quaternion.identity, transform);
            enemy.Init(_target);
        }

        // 화면 사각형을 여유분만큼 키운 테두리 위에서 한 점을 고른다.
        private Vector2 GetSpawnPosition()
        {
            // 기기 화면비마다 보이는 영역이 달라서(Pixel Perfect Camera) 매번 카메라 기준으로 계산한다.
            float halfHeight = _camera.orthographicSize + _spawnMargin;
            float halfWidth = _camera.orthographicSize * _camera.aspect + _spawnMargin;
            Vector2 center = _camera.transform.position;

            // 변 길이에 비례해 위·아래 변과 좌·우 변을 고른다. 세로 화면에서 긴 좌우 변에 더 많이 나와 테두리 전체에 고르게 퍼진다.
            float sign = Random.value < 0.5f ? -1f : 1f;
            if (Random.value < halfWidth / (halfWidth + halfHeight))
            {
                return center + new Vector2(Random.Range(-halfWidth, halfWidth), sign * halfHeight);
            }

            return center + new Vector2(sign * halfWidth, Random.Range(-halfHeight, halfHeight));
        }
    }
}
