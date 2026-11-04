namespace FiveMinuteHero.Weapons
{
    // 강화 쪽이 보는 "가진 무기" 창구. 무기를 어떻게 만들고 보관하는지는 감춘다.
    // 강화는 이 인터페이스만 알아서, 보관 방식이 바뀌어도 강화 코드는 그대로다.
    public interface IWeaponInventory
    {
        // 이 무기를 가졌는지. 무기 강화를 후보에 낼지 정할 때 쓴다.
        bool Has(WeaponData data);

        // 가진 무기의 런타임 스탯. 안 가졌으면 null.
        WeaponRuntimeStats GetStats(WeaponData data);

        // 무기를 새로 얻는다. 데이터만 넘기면 스탯 생성·프리팹 생성·주입은 구현 쪽이 맡는다.
        void Add(WeaponData data);
    }
}
