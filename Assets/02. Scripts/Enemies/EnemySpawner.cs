using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 카메라 화면 바로 바깥에 적을 만들고, 추적 대상을 주입한다.
    // 평소엔 일정 간격으로 만들고, 유지 모드에선 살아 있는 수가 목표에 못 미치는 만큼 채운다(벤치마크용).
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _target;
        [SerializeField] private Enemy[] _enemyPrefabs;
        [SerializeField, Min(0.01f)] private float _spawnInterval = 1f; // 초
        // 원점이 발이라 스프라이트가 위로 약 3유닛 솟는다. 아래쪽에서 나올 때도 안 보이도록 이만큼 띄운다.
        [SerializeField, Min(0f)] private float _spawnMargin = 3f;
        [SerializeField] private EnemyRegistry _registry;

        private float _timer;
        private int _maintainCount;     // 0이면 주기 스폰 모드
        private int _maxSpawnPerFrame;

        // 스폰 전용 난수. UnityEngine.Random은 무기(폭발 대상 고르기) 등도 함께 써서, 고정 시드여도 무기가 몇 번 뽑았는지에 따라 스폰 순서가 밀린다.
        // 자기만 쓰는 난수를 두면 다른 곳이 난수를 몇 번 쓰든 스폰 순서는 시드만으로 정해진다.
        // 기본은 시간 기반 시드라 Main은 지금처럼 매판 무작위다. using System을 쓰면 UnityEngine.Random과 이름이 겹쳐 전체 이름으로 적는다.
        private System.Random _random = new System.Random();

        // 같은 시드면 매번 같은 순서로 스폰한다(벤치마크용). 스폰이 시작되기 전에 부른다.
        public void SetSeed(int seed)
        {
            _random = new System.Random(seed);
        }

        // count > 0이면 그 수를 유지한다. 1000마리를 한 프레임에 만들면 스파이크가 생기므로 프레임당 최대 maxSpawnPerFrame마리만 채운다.
        public void SetMaintainCount(int count, int maxSpawnPerFrame)
        {
            _maintainCount = Mathf.Max(count, 0);
            _maxSpawnPerFrame = Mathf.Max(maxSpawnPerFrame, 1);
        }

        private void Update()
        {
            if (_maintainCount > 0)
            {
                FillToMaintainCount();
                return;
            }

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
            // Next(min, max)도 끝 값을 포함하지 않으므로 Length를 그대로 넘긴다.
            Enemy prefab = _enemyPrefabs[_random.Next(0, _enemyPrefabs.Length)];
            Enemy enemy = Instantiate(prefab, GetSpawnPosition(), Quaternion.identity, transform);
            enemy.Init(_target);

            _registry.Register(enemy);
        }

        private void FillToMaintainCount()
        {
            // 무기에 죽은 만큼 다음 프레임부터 다시 채워져서, 살아 있는 수가 목표 근처에 머문다.
            int toSpawn = Mathf.Min(_maintainCount - _registry.Count, _maxSpawnPerFrame);
            for (int i = 0; i < toSpawn; i++)
            {
                Spawn();
            }
        }

        // 화면 사각형을 여유분만큼 키운 테두리 위에서 한 점을 고른다.
        private Vector2 GetSpawnPosition()
        {
            // 기기 화면비마다 보이는 영역이 달라서(Pixel Perfect Camera) 매번 카메라 기준으로 계산한다.
            float halfHeight = _camera.orthographicSize + _spawnMargin;
            float halfWidth = _camera.orthographicSize * _camera.aspect + _spawnMargin;
            Vector2 center = _camera.transform.position;

            // 변 길이에 비례해 위·아래 변과 좌·우 변을 고른다. 세로 화면에서 긴 좌우 변에 더 많이 나와 테두리 전체에 고르게 퍼진다.
            float sign = NextFloat(0f, 1f) < 0.5f ? -1f : 1f;
            if (NextFloat(0f, 1f) < halfWidth / (halfWidth + halfHeight))
            {
                return center + new Vector2(NextFloat(-halfWidth, halfWidth), sign * halfHeight);
            }

            return center + new Vector2(sign * halfWidth, NextFloat(-halfHeight, halfHeight));
        }

        // System.Random에는 float 범위 함수가 없어서 0~1 값을 [min, max) 범위로 늘린다.
        private float NextFloat(float min, float max)
        {
            return min + (float)_random.NextDouble() * (max - min);
        }
    }
}
