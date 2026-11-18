using System;
using System.Collections.Generic;
using FiveMinuteHero.Enemies;
using UnityEngine;

namespace FiveMinuteHero.Stages
{
    // 한 판 안의 시간 구간 하나. 시작 시각부터 다음 웨이브 시작 전까지 이 설정으로 적이 나온다.
    // 에셋(SO)이 아니라 StageData 안 목록의 한 칸이다. 웨이브마다 에셋을 만들면 스테이지 수 × 웨이브 수만큼 파일이 늘어난다.
    // 숫자는 전부 여기(SO 안)에만 둔다. 나중에 난이도 배율을 곱할 때도 이 값들을 읽어 곱하기만 하면 된다.
    [Serializable]
    public class WaveData
    {
        [SerializeField, Min(0f)] private float _startSeconds;          // 판 시작부터 몇 초에 이 웨이브가 시작되는지
        [SerializeField, Min(0.01f)] private float _spawnInterval = 1f; // 몇 초마다 1마리씩 나오는지
        [SerializeField, Min(1)] private int _maxAlive = 15;            // 동시에 살아 있을 수 있는 최대 수. 이만큼 있으면 자리가 날 때까지 쉰다
        // 이 웨이브에 나올 적 프리팹. 균등하게 뽑으므로, 더 자주 나오게 하려면 같은 프리팹을 여러 번 넣는다.
        // 스포너의 _enemyPrefabs에 있는 프리팹만 쓸 수 있다(풀이 그 목록 기준으로 만들어진다).
        [SerializeField] private Enemy[] _enemies;

        public float StartSeconds => _startSeconds;
        public float SpawnInterval => _spawnInterval;
        public int MaxAlive => _maxAlive;
        // 밖에서 배열 내용을 바꾸지 못하게 읽기 전용으로 내보낸다(SO 원본은 수정하지 않는다).
        public IReadOnlyList<Enemy> Enemies => _enemies;
    }
}
