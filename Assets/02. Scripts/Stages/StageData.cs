using System.Collections.Generic;
using UnityEngine;

namespace FiveMinuteHero.Stages
{
    // 스테이지 하나의 데이터. 스테이지마다 에셋을 하나씩 만든다(15단계를 에셋 15개로 복제하지 않는다).
    // 판 길이와, 시간에 따라 적이 어떻게 나올지(웨이브 목록)를 한 에셋에서 본다.
    [CreateAssetMenu(fileName = "StageData", menuName = "FiveMinuteHero/Stage Data")]
    public class StageData : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _durationSeconds = 300f; // 이 시간을 버티면 클리어(초)
        // 시작 시각 오름차순으로 적는다. 첫 웨이브는 0초여야 판 시작부터 적이 나온다(StageRunner가 시작할 때 검사한다).
        [SerializeField] private WaveData[] _waves;

        public float DurationSeconds => _durationSeconds;
        public IReadOnlyList<WaveData> Waves => _waves;
    }
}
