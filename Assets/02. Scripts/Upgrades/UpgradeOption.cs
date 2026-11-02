namespace FiveMinuteHero.Upgrades
{
    // 카드 한 장에 보여 줄 선택지: 어떤 강화를 몇 레벨로 올리는지.
    // 값 두 개짜리 작은 묶음이라 struct로 둬서 레벨업마다 만들어도 힙 할당이 없다. readonly라 View가 바꿀 수 없다.
    public readonly struct UpgradeOption
    {
        public UpgradeOption(UpgradeData data, int nextLevel)
        {
            Data = data;
            NextLevel = nextLevel;
        }

        public UpgradeData Data { get; }
        public int NextLevel { get; }
    }
}
