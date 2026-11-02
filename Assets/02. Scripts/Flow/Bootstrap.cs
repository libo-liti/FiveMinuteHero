using FiveMinuteHero.Player;
using FiveMinuteHero.Upgrades;
using FiveMinuteHero.Weapons;
using UnityEngine;

namespace FiveMinuteHero.Flow
{
    // 씬마다 하나. 런타임 스탯을 SO로 만들고, 필요한 컴포넌트에 Init으로 넘긴다(방식 B: 수동 주입).
    // 다른 컴포넌트의 Awake보다 먼저 돌도록 실행 순서를 앞당긴다. Unity는 서로 다른 오브젝트의 Awake 순서를 보장하지 않는다.
    //
    // 주입받는 쪽 규칙:
    // 1. Init이 생성자 역할이다. 주입받은 값으로 하는 초기화는 전부 Init에서 한다.
    // 2. Awake는 자기 컴포넌트 캐싱만 하고, Init이 넣은 값을 덮어쓰지 않는다.
    // 3. Init은 Awake보다 먼저 올 수 있으니, Init 안에서 Awake가 캐싱하는 필드를 쓰지 않는다.
    // 4. Start에서 Init을 받았는지 확인하고, 못 받았으면 Debug.LogError(this)로 알린다.
    [DefaultExecutionOrder(-100)]
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private WeaponData _magicBoltData;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private PlayerExperience _playerExperience;
        [SerializeField] private MagicBoltWeapon _magicBolt;
        [SerializeField] private GameSession _gameSession;
        [SerializeField] private LevelUpController _levelUpController;

        private void Awake()
        {
            // 이동·체력·경험치가 같은 객체를 받아야 강화 하나가 세 곳에 함께 반영된다.
            PlayerRuntimeStats playerStats = new PlayerRuntimeStats(_playerStats);
            WeaponRuntimeStats magicBoltStats = new WeaponRuntimeStats(_magicBoltData);

            _playerMovement.Init(playerStats);
            _playerHealth.Init(playerStats);
            _playerExperience.Init(playerStats);
            _magicBolt.Init(magicBoltStats);

            // 강화가 바꿀 대상을 모은다. 무기가 늘면 가진 무기마다 RegisterWeapon을 부른다.
            UpgradeContext context = new UpgradeContext(playerStats);
            context.RegisterWeapon(_magicBoltData, magicBoltStats);

            // Benchmark 씬에는 레벨업이 없어서 비워 둔다. GameSession은 IPauseService로 넘겨 컨트롤러가 Restart 등을 모르게 한다.
            if (_levelUpController != null)
            {
                _levelUpController.Init(context, _gameSession);
            }
        }
    }
}
