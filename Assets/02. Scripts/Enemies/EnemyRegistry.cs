using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 살아 있는 적 목록을 들고, 죽은 적은 스스로 빼고, 그 죽음을 다른 시스템에 대신 알린다. 적을 만들지는 않는다.
    public class EnemyRegistry : MonoBehaviour
    {
        private readonly List<Enemy> _aliveEnemies = new List<Enemy>();

        // 무기가 가장 가까운 적을 찾을 때 순회한다. 밖에서는 읽기만 하게 막는다.
        public IReadOnlyList<Enemy> AliveEnemies => _aliveEnemies;
        // 성능 측정 도구가 현재 적 수를 읽는다.
        public int Count => _aliveEnemies.Count;
        // 판 시작부터 죽은 적의 누적 수. 성능 측정 도구가 구간 시작·끝의 차이로 그 구간의 처치 수를 낸다.
        public int DiedCount { get; private set; }
        // 등록된 적이 죽을 때마다 알린다(젬 드롭 등). 적의 Died를 직접 구독하는 곳은 이 레지스트리 하나뿐이다.
        // 구독자는 이 이벤트에 한 번만 붙으면 되므로, 적이 재사용돼도 구독이 쌓이지 않는다.
        public event Action<Enemy> EnemyDied;

        public void Register(Enemy enemy)
        {
            _aliveEnemies.Add(enemy);
            enemy.Died += OnEnemyDied;
        }

        private void OnEnemyDied(Enemy enemy)
        {
            // 등록 때 붙인 구독을 여기서 뗀다. 같은 적이 다시 등록돼도 이 핸들러는 한 번만 불린다.
            enemy.Died -= OnEnemyDied;
            // 목록에서 빼지 않으면 무기가 죽은 적을 조준한다.
            _aliveEnemies.Remove(enemy);
            DiedCount++;
            // 집계를 마친 뒤 알린다. 구독자는 아직 이 적의 위치를 읽을 수 있다.
            EnemyDied?.Invoke(enemy);
        }
    }
}
