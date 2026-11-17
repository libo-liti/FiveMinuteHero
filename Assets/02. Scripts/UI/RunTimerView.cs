using FiveMinuteHero.Flow;
using TMPro;
using UnityEngine;

namespace FiveMinuteHero.UI
{
    // 화면 위 남은 시간(m:ss). StageRunner의 이벤트만 구독해서 그리고, 시간 계산은 하지 않는다.
    public class RunTimerView : MonoBehaviour
    {
        private const int SecondsPerMinute = 60;
        // {1:00}은 두 자리로 채운다(5:07). SetText가 이 형식을 직접 해석해서 새 문자열을 만들지 않는다.
        private const string TimeFormat = "{0}:{1:00}";

        [SerializeField] private StageRunner _stageRunner;
        [SerializeField] private TMP_Text _timeText;

        private void OnEnable()
        {
            _stageRunner.RemainingSecondsChanged += OnRemainingSecondsChanged;
        }

        private void OnDisable()
        {
            _stageRunner.RemainingSecondsChanged -= OnRemainingSecondsChanged;
        }

        private void Start()
        {
            // 이벤트는 남은 초가 처음 바뀔 때(1초 뒤)까지 안 오므로, 시작 값(5:00)은 직접 한 번 그린다.
            OnRemainingSecondsChanged(_stageRunner.RemainingSeconds);
        }

        private void OnRemainingSecondsChanged(int remainingSeconds)
        {
            int minutes = remainingSeconds / SecondsPerMinute;
            int seconds = remainingSeconds % SecondsPerMinute;
            // $"{minutes}:{seconds:00}"는 부를 때마다 새 string을 만든다(GC 할당).
            // SetText는 숫자를 TMP 안의 문자 배열에 바로 써서 할당이 없다.
            _timeText.SetText(TimeFormat, minutes, seconds);
        }
    }
}
