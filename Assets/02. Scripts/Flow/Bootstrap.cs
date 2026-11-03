using FiveMinuteHero.Enemies;
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
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private PlayerExperience _playerExperience;
        [SerializeField] private GameSession _gameSession;
        [SerializeField] private LevelUpController _levelUpController;

        // 판을 시작할 때 가진 무기. 씬마다 다르게 둘 수 있다(Benchmark는 측정 조건 때문에 마법탄만).
        [SerializeField] private WeaponData[] _startingWeapons;
        [SerializeField] private Transform _weaponMount;       // 무기를 붙일 플레이어 자식(몸 높이)
        [SerializeField] private EnemyRegistry _enemyRegistry; // 무기가 적을 찾을 때 쓴다. 프리팹 무기도 받을 수 있게 Init으로 넘긴다

        private void Awake()
        {
            // 이동·체력·경험치가 같은 객체를 받아야 강화 하나가 세 곳에 함께 반영된다.
            PlayerRuntimeStats playerStats = new PlayerRuntimeStats(_playerStats);

            _playerMovement.Init(playerStats);
            _playerHealth.Init(playerStats);
            _playerExperience.Init(playerStats);

            // 가진 무기 목록. 시작 무기도 판 중간 획득과 같은 Add 경로로 만든다.
            WeaponInventory weapons = new WeaponInventory(_weaponMount, _enemyRegistry);
            foreach (WeaponData weapon in _startingWeapons)
            {
                weapons.Add(weapon);
            }

            // 강화가 바꿀 대상을 모은다.
            UpgradeContext context = new UpgradeContext(playerStats, weapons);

            // Benchmark 씬에는 레벨업이 없어서 비워 둔다. GameSession은 IPauseService로 넘겨 컨트롤러가 Restart 등을 모르게 한다.
            if (_levelUpController != null)
            {
                _levelUpController.Init(context, _gameSession);
            }
        }
    }
}
