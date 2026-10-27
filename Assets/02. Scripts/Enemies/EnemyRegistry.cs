using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiveMinuteHero.Enemies
{
    // 살아 있는 적 목록을 들고, 등록을 알리며, 죽은 적은 스스로 뺀다. 적을 만들지는 않는다.
    public class EnemyRegistry : MonoBehaviour
    {
        private readonly List<Enemy> _aliveEnemies = new List<Enemy>();

        // 무기가 가장 가까운 적을 찾을 때 순회한다. 밖에서는 읽기만 하게 막는다.
        public IReadOnlyList<Enemy> AliveEnemies => _aliveEnemies;
        // 성능 측정 도구가 현재 적 수를 읽는다.
        public int Count => _aliveEnemies.Count;
        // 젬 드롭 등이 새 적의 Died를 구독할 수 있게 알린다. 레지스트리는 구독자가 뭘 하는지 모른다.
        public event Action<Enemy> Registered;

        public void Register(Enemy enemy)
        {
            _aliveEnemies.Add(enemy);
            enemy.Died += OnEnemyDied;
            Registered?.Invoke(enemy);
        }

        private void OnEnemyDied(Enemy enemy)
        {
            // 목록에서 빼지 않으면 무기가 파괴된 적을 조준한다.
            _aliveEnemies.Remove(enemy);
        }
    }
}
