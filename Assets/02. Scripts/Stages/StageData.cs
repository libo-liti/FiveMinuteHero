using UnityEngine;

namespace FiveMinuteHero.Stages
{
    // 스테이지 하나의 데이터. 스테이지마다 에셋을 하나씩 만든다(15단계를 에셋 15개로 복제하지 않는다).
    // 지금은 판 길이만 있고, 다음 PR에서 웨이브 목록이 들어온다.
    [CreateAssetMenu(fileName = "StageData", menuName = "FiveMinuteHero/Stage Data")]
    public class StageData : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _durationSeconds = 300f; // 이 시간을 버티면 클리어(초)

        public float DurationSeconds => _durationSeconds;
    }
}
