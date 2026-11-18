namespace FiveMinuteHero.Stages
{
    // 웨이브를 넣을 수 있는 쪽. StageRunner는 이 창구로만 웨이브를 넘긴다.
    // 스포너를 직접 받으면 시드·유지 모드(벤치마크 전용) 같은 다른 기능까지 부를 수 있게 되므로, 필요한 메서드 하나만 연다.
    public interface IWaveTarget
    {
        void ApplyWave(WaveData wave);
    }
}
