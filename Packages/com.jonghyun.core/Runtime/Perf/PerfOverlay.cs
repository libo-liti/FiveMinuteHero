using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jonghyun.Core.Perf
{
    // PerfSampler의 실시간 값을 화면에 그리고, 측정이 끝나면 결과를 보여 준다.
    // 실시간 갱신은 TMP SetText(서식, 숫자)로 해서 문자열을 만들지 않는다(측정 중 할당 0).
    // SetText는 숫자를 최대 8개까지만 받고 값 자리에 글자를 넣을 수 없어서, 텍스트를 3개로 나누고
    // "미지원"이 될 수 있는 값은 서식 문자열 자체를 바꿔 표시한다. 서식은 모두 상수라 바꿔도 할당이 없다.
    public class PerfOverlay : MonoBehaviour
    {
        private const float RefreshInterval = 0.5f; // 초 (unscaled). 매 프레임 갱신하면 텍스트 메시 재생성 비용이 측정에 섞인다.
        private const float BytesPerMegabyte = 1024f * 1024f;

        // 프레임: {0}=FPS {1}=CPU 메인 ms {2}=CPU 렌더 ms
        private const string FrameFormat = "FPS {0:0}\nCPU main {1:1} ms\nCPU render {2:1} ms";
        private const string FrameFormatNoCpu = "FPS {0:0}\nCPU 미지원";
        // GPU: {0}=GPU ms
        private const string GpuFormat = "GPU {0:1} ms";
        private const string GpuUnavailableText = "GPU 미지원";
        // 그리기·메모리: 인자는 항상 6개를 같은 순서로 넘기고, 미지원인 값은 서식에서 그 번호를 빼고 글자로 쓴다.
        // {0}=Batches {1}=SetPass {2}=Total mem MB {3}=GC mem MB {4}=GC count {5}=측정 경과 초
        private const string DrawMemoryFormat =
            "Batches {0:0}\nSetPass {1:0}\nMem {2:1} MB\nGC mem {3:1} MB\nGC count {4:0}\nREC {5:0}s";
        private const string DrawMemoryFormatNoRender =
            "Batches 미지원\nSetPass 미지원\nMem {2:1} MB\nGC mem {3:1} MB\nGC count {4:0}\nREC {5:0}s";
        private const string DrawMemoryFormatNoTotalMem =
            "Batches {0:0}\nSetPass {1:0}\nMem 미지원\nGC mem {3:1} MB\nGC count {4:0}\nREC {5:0}s";
        private const string DrawMemoryFormatNoRenderNoTotalMem =
            "Batches 미지원\nSetPass 미지원\nMem 미지원\nGC mem {3:1} MB\nGC count {4:0}\nREC {5:0}s";

        // 기존 씬의 _text 연결을 잃지 않도록 이름만 바꾼다.
        [FormerlySerializedAs("_text")]
        [SerializeField] private TMP_Text _frameText;
        [SerializeField] private TMP_Text _gpuText;
        [SerializeField] private TMP_Text _drawMemoryText;

        private PerfSampler _sampler;
        private float _timer;
        private bool _isShowingReport;
        private string _drawMemoryFormat = DrawMemoryFormat;

        public void Bind(PerfSampler sampler)
        {
            _sampler = sampler;
            _isShowingReport = false;
            _timer = RefreshInterval; // 다음 프레임에 바로 한 번 그린다.
            // recorder의 Valid는 만든 뒤 바뀌지 않으므로 서식을 여기서 한 번만 고른다.
            _drawMemoryFormat = SelectDrawMemoryFormat(sampler.HasRenderStats, sampler.HasTotalUsedMemory);
        }

        // 측정이 끝난 뒤에만 부르므로 문자열 할당이 결과에 영향을 주지 않는다.
        // 결과는 여러 줄이라 첫 텍스트에 모두 쓰고, 나머지는 비운다.
        public void ShowReport(PerfReport report, string label)
        {
            _isShowingReport = true;
            _frameText.text = report.ToDisplayText(label);
            _gpuText.text = string.Empty;
            _drawMemoryText.text = string.Empty;
        }

        private void Start()
        {
            if (_frameText == null || _gpuText == null || _drawMemoryText == null)
            {
                Debug.LogError("PerfOverlay: 텍스트 3개(_frameText, _gpuText, _drawMemoryText)가 모두 연결되어야 합니다.", this);
            }
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
            // 0ms는 "아주 빠름"이 아니라 "못 잼"이므로 미지원으로 표시한다.
            bool hasCpu = _sampler.IsFrameTimingEnabled && _sampler.LiveCpuMainMs > 0f && _sampler.LiveCpuRenderMs > 0f;
            if (hasCpu)
            {
                _frameText.SetText(FrameFormat, _sampler.LiveFps, _sampler.LiveCpuMainMs, _sampler.LiveCpuRenderMs);
            }
            else
            {
                _frameText.SetText(FrameFormatNoCpu, _sampler.LiveFps);
            }

            bool hasGpu = _sampler.IsFrameTimingEnabled && _sampler.LiveGpuMs > 0f;
            if (hasGpu)
            {
                _gpuText.SetText(GpuFormat, _sampler.LiveGpuMs);
            }
            else
            {
                _gpuText.SetText(GpuUnavailableText);
            }

            _drawMemoryText.SetText(_drawMemoryFormat,
                _sampler.LiveBatches,
                _sampler.LiveSetPassCalls,
                _sampler.LiveTotalUsedMemory / BytesPerMegabyte,
                _sampler.LiveGcUsedMemory / BytesPerMegabyte,
                _sampler.LiveGcCount,
                _sampler.Elapsed);
        }

        private static string SelectDrawMemoryFormat(bool hasRenderStats, bool hasTotalUsedMemory)
        {
            if (hasRenderStats)
            {
                return hasTotalUsedMemory ? DrawMemoryFormat : DrawMemoryFormatNoTotalMem;
            }

            return hasTotalUsedMemory ? DrawMemoryFormatNoRender : DrawMemoryFormatNoRenderNoTotalMem;
        }
    }
}
