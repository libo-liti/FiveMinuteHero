using TMPro;
using UnityEngine;

namespace Jonghyun.Core.Perf
{
    // PerfSampler의 실시간 값을 화면에 그리고, 측정이 끝나면 결과를 보여 준다.
    // 실시간 갱신은 TMP SetText(서식, 숫자)로 해서 문자열을 만들지 않는다(측정 중 할당 0).
    public class PerfOverlay : MonoBehaviour
    {
        private const float RefreshInterval = 0.5f; // 초 (unscaled). 매 프레임 갱신하면 텍스트 메시 재생성 비용이 측정에 섞인다.
        private const float BytesPerMegabyte = 1024f * 1024f;

        [SerializeField] private TMP_Text _text;

        private PerfSampler _sampler;
        private float _timer;
        private bool _isShowingReport;

        public void Bind(PerfSampler sampler)
        {
            _sampler = sampler;
            _isShowingReport = false;
            _timer = RefreshInterval; // 다음 프레임에 바로 한 번 그린다.
        }

        // 측정이 끝난 뒤에만 부르므로 문자열 할당이 결과에 영향을 주지 않는다.
        public void ShowReport(PerfReport report, string label)
        {
            _isShowingReport = true;
            _text.text = report.ToDisplayText(label);
        }

        private void Update()
        {
            if (_sampler == null || _isShowingReport)
            {
                return;
            }

            // timeScale을 0으로 멈춰도 갱신되도록 unscaled 시간을 쓴다.
            _timer += Time.unscaledDeltaTime;
            if (_timer < RefreshInterval)
            {
                return;
            }
            _timer = 0f;

            // TMP 서식 {n:d}는 소수점 자리 수다. 실시간 값은 측정 구간이 아니어도 계속 보여 준다.
            _text.SetText("FPS {0:0}\nMem {1:1} MB\nGC mem {2:1} MB\nGC count {3:0}\nREC {4:0}s",
                _sampler.LiveFps,
                _sampler.LiveTotalUsedMemory / BytesPerMegabyte,
                _sampler.LiveGcUsedMemory / BytesPerMegabyte,
                _sampler.LiveGcCount,
                _sampler.Elapsed);
        }
    }
}
