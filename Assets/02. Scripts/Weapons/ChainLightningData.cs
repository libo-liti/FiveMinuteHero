using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 연쇄 번개 전용 기본값. 쿨다운과 사거리는 기존 스탯을 그대로 쓰고, 튀는 횟수와 튀는 거리만 새 스탯이다.
    [CreateAssetMenu(fileName = "ChainLightningData", menuName = "FiveMinuteHero/Weapons/Chain Lightning Data")]
    public class ChainLightningData : WeaponData
    {
        // 쿨다운이 0이 되면 매 프레임 공격하게 된다.
        private const float MinCooldown = 0.05f;
        private const float MinRange = 0.1f;
        // 0이면 첫 목표만 맞히는 단일 공격이 된다. 연쇄라는 무기의 정체성을 지키려고 최소 1번은 튄다.
        private const int MinChainCount = 1;
        // 튀는 거리가 0이면 다음 적을 찾을 수 없어 튀는 횟수 강화가 의미 없어진다.
        private const float MinChainRange = 0.1f;

        [SerializeField, Min(MinCooldown)] private float _cooldown = 2f;        // 공격 간격(초)
        [SerializeField, Min(MinRange)] private float _range = 6f;              // 첫 목표로 고를 수 있는 적까지의 거리(유닛)
        [SerializeField, Min(MinChainCount)] private int _chainCount = 3;       // 첫 목표 다음으로 튀는 횟수. 최대 (1 + 이 값)명을 맞힌다
        [SerializeField, Min(MinChainRange)] private float _chainRange = 3f;    // 직전에 맞은 적에서 다음 적까지의 최대 거리(유닛)

        protected override void DefineStats(WeaponRuntimeStats stats)
        {
            stats.Define(WeaponStatType.Cooldown, _cooldown, MinCooldown);
            stats.Define(WeaponStatType.Range, _range, MinRange);
            stats.Define(WeaponStatType.ChainCount, _chainCount, MinChainCount);
            stats.Define(WeaponStatType.ChainRange, _chainRange, MinChainRange);
        }
    }
}
