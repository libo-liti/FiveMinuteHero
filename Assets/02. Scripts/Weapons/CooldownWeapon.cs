using UnityEngine;

namespace FiveMinuteHero.Weapons
{
    // 쿨타임마다 공격하는 무기의 공통 흐름(템플릿 메서드).
    // 흐름(쿨다운 → 공격 시도 → 대기 또는 쿨다운 소모)은 여기서 고정하고, 공격 한 번이 무엇인지는 하위 클래스가 TryAttack으로 채운다.
    // 회전 오브처럼 쿨다운이 없는 무기도 있어서 Weapon이 아니라 이 중간 클래스에 둔다.
    public abstract class CooldownWeapon : Weapon
    {
        private float _timer;

        // 쿨다운 무기라면 데이터(DefineStats)에 Cooldown 스탯이 있어야 한다. Start에서 한 번 확인한다.
        // 강화로 값이 바뀔 수 있어서 저장하지 않고 쓸 때마다 읽는다.
        private float Cooldown => Stats.Get(WeaponStatType.Cooldown);

        protected override void Start()
        {
            base.Start();
            // Init을 못 받아 부모가 꺼 둔 경우. 이대로 진행하면 Stats가 null이다.
            if (!enabled)
            {
                return;
            }

            // Get은 없는 스탯을 읽을 때마다 에러를 남기므로, 그대로 두면 매 프레임 에러가 쌓인다.
            // 시작할 때 한 번만 알리고 무기를 멈춘다(Init 확인과 같은 방식).
            if (!Stats.Has(WeaponStatType.Cooldown))
            {
                Debug.LogError($"{GetType().Name}: 쿨다운 무기인데 Cooldown 스탯이 없습니다. 연결된 WeaponData의 DefineStats를 확인하세요.", this);
                enabled = false;
            }
        }

        // 하위 클래스는 Update를 선언하지 않는다. 선언하면 Unity가 이 Update 대신 그쪽만 불러 쿨다운이 돌지 않는다.
        private void Update()
        {
            // 레벨업 창 등으로 timeScale이 0이면 deltaTime도 0이라서 타이머가 멈춘다.
            _timer += Time.deltaTime;
            if (_timer < Cooldown)
            {
                return;
            }

            if (!TryAttack())
            {
                // 쿨타임이 찬 채로 기다리다가, 대상이 생기는 프레임에 바로 공격한다.
                _timer = Cooldown;
                return;
            }

            // 0으로 되돌리지 않고 빼서, 이번 프레임에 넘친 시간을 다음 쿨다운에 이어 쓴다.
            _timer -= Cooldown;
        }

        // 공격을 한 번 시도한다. 목표를 찾는 것부터 공격까지 하위 클래스가 정한다.
        // true  = 이번에 공격했다 → 쿨다운을 한 번 소모한다.
        // false = 이번엔 공격할 대상이 없다 → 쿨다운을 쓰지 않고 찬 채로 기다린다.
        //         false를 돌려줄 때는 발사·피해·이펙트 같은 효과를 하나도 내면 안 된다.
        protected abstract bool TryAttack();
    }
}
