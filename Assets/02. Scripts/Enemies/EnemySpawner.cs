using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace FiveMinuteHero.Enemies
{
    // 카메라 화면 바로 바깥에 적을 꺼내 놓고, 추적 대상을 주입한다. 죽은 적은 파괴하지 않고 풀에 돌려놓았다가 다시 쓴다.
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

        // 종류마다 풀 하나. _enemyPrefabs와 같은 순서라서 난수로 고른 번호를 그대로 풀 번호로 쓴다.
        private ObjectPool<Enemy>[] _pools;
        // 반납할 때 어느 풀로 돌려보낼지 알아야 한다. 적은 자기 종류 번호를 모르므로 만든 쪽이 기억한다.
        private readonly Dictionary<Enemy, int> _typeIndexByEnemy = new Dictionary<Enemy, int>();
        // 이번 프레임에 죽어서 LateUpdate에 반납할 적.
        private readonly List<Enemy> _pendingRelease = new List<Enemy>();

        private float _timer;
        private int _maintainCount;     // 0이면 주기 스폰 모드
        private int _maxSpawnPerFrame;

        // 스폰 전용 난수. UnityEngine.Random은 무기(폭발 대상 고르기) 등도 함께 써서, 고정 시드여도 무기가 몇 번 뽑았는지에 따라 스폰 순서가 밀린다.
        // 자기만 쓰는 난수를 두면 다른 곳이 난수를 몇 번 쓰든 스폰 순서는 시드만으로 정해진다.
        // 기본은 시간 기반 시드라 Main은 지금처럼 매판 무작위다. using System을 쓰면 UnityEngine.Random과 이름이 겹쳐 전체 이름으로 적는다.
        private System.Random _random = new System.Random();

        private void Awake()
        {
            _pools = new ObjectPool<Enemy>[_enemyPrefabs.Length];
            for (int i = 0; i < _enemyPrefabs.Length; i++)
            {
                // 람다가 반복 변수 i를 직접 잡으면 모든 풀이 반복이 끝난 뒤의 i를 보게 된다. 지역 변수에 복사해 풀마다 따로 잡는다.
                int typeIndex = i;
                _pools[i] = new ObjectPool<Enemy>(
                    createFunc: () => CreateEnemy(typeIndex),
                    actionOnRelease: ReleaseEnemy,
                    // 같은 적을 두 번 반납하면 에디터에서 에러로 잡는다. 검사가 풀 크기만큼 순회하므로 빌드(측정)에서는 끈다.
                    collectionCheck: Application.isEditor);
            }

            // 스포너가 꺼진 채 시작하는 씬(Benchmark)도 있어서, OnEnable 짝이 아니라 수명 전체에 걸쳐 구독한다.
            _registry.EnemyDied += OnEnemyDied;
        }

        private void OnDestroy()
        {
            _registry.EnemyDied -= OnEnemyDied;
        }

        // 같은 시드면 매번 같은 순서로 스폰한다(벤치마크용). 스폰이 시작되기 전에 부른다.
        public void SetSeed(int seed)
        {
            _random = new System.Random(seed);
        }

        // count > 0이면 그 수를 유지한다. 1000마리를 한 프레임에 만들면 스파이크가 생기므로 프레임당 최대 maxSpawnPerFrame마리만 채운다.
        // 유지 모드는 측정용이라, 측정 중에 Instantiate가 섞이지 않도록 여기서(측정 시작 전) 필요한 적을 미리 만들어 둔다.
        public void SetMaintainCount(int count, int maxSpawnPerFrame)
        {
            _maintainCount = Mathf.Max(count, 0);
            _maxSpawnPerFrame = Mathf.Max(maxSpawnPerFrame, 1);

            if (_maintainCount > 0)
            {
                // 반납은 프레임 끝이라, 이번 프레임에 죽은 적이 아직 돌아오지 않은 채 스포너가 최대 maxSpawnPerFrame마리를 더 꺼낼 수 있다.
                // 어느 종류가 나올지는 난수라 모르므로, 한 종류만 계속 나와도 모자라지 않게 종류마다 이만큼 둔다.
                Prewarm(_maintainCount + _maxSpawnPerFrame);
            }
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

        // Destroy가 처리되던 시점(이번 프레임의 Update 이후, 렌더 이전)과 맞춰 반납한다.
        // Died 순간에 바로 끄면, 같은 물리 스텝에서 그 적에 닿은 두 번째 탄이 콜백을 못 받아 뚫고 지나가는 등 동작이 달라진다.
        private void LateUpdate()
        {
            for (int i = 0; i < _pendingRelease.Count; i++)
            {
                Enemy enemy = _pendingRelease[i];
                _pools[_typeIndexByEnemy[enemy]].Release(enemy);
            }

            _pendingRelease.Clear();
        }

        private void Spawn()
        {
            // 난수를 쓰는 순서(종류 → 위치)를 풀링 전과 같게 둔다. 그래야 같은 시드에서 스폰 순서가 같다.
            // Next(min, max)도 끝 값을 포함하지 않으므로 Length를 그대로 넘긴다.
            int typeIndex = _random.Next(0, _enemyPrefabs.Length);
            Vector2 position = GetSpawnPosition();

            Enemy enemy = _pools[typeIndex].Get();
            // 꺼진 상태에서 옮겨야 켜질 때 물리 바디가 새 위치에서 시작한다. 보간이 옛 위치(죽은 자리)에서 끌려오지 않는다.
            enemy.transform.position = position;
            // 풀링 전과 같은 순서(켜짐 → Init → 등록)를 지킨다.
            enemy.gameObject.SetActive(true);
            enemy.Init(_target);

            _registry.Register(enemy);
        }

        // 풀이 비어 있을 때만 불린다. 처음 만들 때도 꺼 둔 채 풀에 넣어, 꺼낼 때의 흐름을 새 적과 재사용 적이 같게 한다.
        private Enemy CreateEnemy(int typeIndex)
        {
            Enemy enemy = Instantiate(_enemyPrefabs[typeIndex], transform);
            enemy.gameObject.SetActive(false);
            _typeIndexByEnemy.Add(enemy, typeIndex);
            return enemy;
        }

        // 끄기만 한다. 깜빡임·속도 같은 자기 상태는 각 컴포넌트가 OnDisable에서 스스로 되돌린다.
        private static void ReleaseEnemy(Enemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }

        // ObjectPool에는 미리 만들기 기능이 없다. 비어 있는 풀에서 한꺼번에 꺼내면 모자란 만큼 createFunc가 불려 새로 만들어지고,
        // 그것을 다시 모두 반납하면 풀 안에 꺼진 적이 쌓인다. 꺼낸 적은 켜지 않으므로 화면·물리에는 나오지 않는다.
        // 난수(_random)를 쓰지 않아서 같은 시드의 스폰 순서에 영향이 없다.
        private void Prewarm(int countPerType)
        {
            // 버튼을 누른 순간 한 번만 불리므로 임시 리스트 할당은 측정과 무관하다.
            List<Enemy> taken = new List<Enemy>(countPerType);
            foreach (ObjectPool<Enemy> pool in _pools)
            {
                for (int i = 0; i < countPerType; i++)
                {
                    taken.Add(pool.Get());
                }

                for (int i = 0; i < taken.Count; i++)
                {
                    pool.Release(taken[i]);
                }

                taken.Clear();
            }
        }

        private void OnEnemyDied(Enemy enemy)
        {
            _pendingRelease.Add(enemy);
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
