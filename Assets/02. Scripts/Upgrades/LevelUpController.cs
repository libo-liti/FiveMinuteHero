using System;
using System.Collections.Generic;
using FiveMinuteHero.Flow;
using FiveMinuteHero.Player;
using UnityEngine;

namespace FiveMinuteHero.Upgrades
{
    // 레벨업하면 게임을 멈추고 강화 후보를 뽑아 View에 알리고, 고른 강화를 적용한 뒤 재개한다.
    // 화면은 모른다. View가 ChoicesOffered를 듣고 카드를 띄운 뒤 Choose(index)로 결과를 돌려준다.
    public class LevelUpController : MonoBehaviour
    {
        private const int ChoiceCount = 3;

        [SerializeField] private UpgradeData[] _upgradePool; // 이번 판에 나올 수 있는 강화 전체
        [SerializeField] private PlayerExperience _experience;
        [SerializeField] private PlayerHealth _playerHealth;

        private UpgradeContext _context;
        private IPauseService _pauseService;

        private readonly Dictionary<UpgradeData, int> _levels = new Dictionary<UpgradeData, int>(); // 강화별 현재 레벨. 없으면 0
        private int _pendingCount;  // 아직 고르지 않은 레벨업 수
        private bool _isChoosing;   // 창이 떠 있고 정지를 요청해 둔 상태

        // 후보 뽑기에 매번 새 리스트를 만들지 않도록 비우고 다시 쓴다.
        private readonly List<UpgradeData> _available = new List<UpgradeData>();
        private readonly List<UpgradeOption> _options = new List<UpgradeOption>();

        // 보여 줄 선택지 목록. View가 구독한다. 읽기 전용으로 넘겨 View가 목록을 바꾸지 못하게 한다.
        public event Action<IReadOnlyList<UpgradeOption>> ChoicesOffered;

        // Bootstrap이 호출한다. 저장만 하고 Awake 캐싱 필드에는 기대지 않는다.
        public void Init(UpgradeContext context, IPauseService pauseService)
        {
            _context = context;
            _pauseService = pauseService;
        }

        private void Start()
        {
            if (_context == null || _pauseService == null)
            {
                Debug.LogError("LevelUpController: Init을 받지 못했습니다. 씬의 Bootstrap 연결을 확인하세요.", this);
                // 꺼 두면 OnDisable에서 구독이 풀려 레벨업 때 NullReference가 나지 않는다.
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _experience.LeveledUp += OnLeveledUp;
        }

        private void OnDisable()
        {
            _experience.LeveledUp -= OnLeveledUp;
        }

        // View가 카드 클릭 시 호출한다. index는 ChoicesOffered로 넘긴 목록의 위치.
        public void Choose(int index)
        {
            // 버튼 연타 등으로 창이 닫힌 뒤 또 불리거나, 목록 밖의 번호가 오면 막는다.
            if (!_isChoosing || index < 0 || index >= _options.Count)
            {
                Debug.LogError($"LevelUpController: 지금은 고를 수 없거나 잘못된 번호입니다: {index}", this);
                return;
            }

            // 다음 레벨은 다시 계산하지 않고 옵션에 담긴 값을 쓴다(보여 준 것과 적용한 것이 같아야 한다).
            UpgradeOption option = _options[index];
            option.Data.Apply(_context, option.NextLevel);
            _levels[option.Data] = option.NextLevel;

            _pendingCount--;
            if (_pendingCount > 0)
            {
                ShowNextChoices();
            }
            else
            {
                FinishChoosing();
            }
        }

        private void OnLeveledUp(int level)
        {
            // 죽은 뒤 흡수된 젬으로 게임 오버 패널 위에 창이 뜨지 않게 한다.
            if (_playerHealth.IsDead)
            {
                return;
            }

            _pendingCount++;

            // 정지 중에도 젬 흡수는 돌아서 창이 떠 있을 때 또 올 수 있다. 개수만 늘리면 Choose가 이어서 보여 준다.
            if (_isChoosing)
            {
                return;
            }

            // 정지 요청은 레벨 수가 아니라 창을 여는 한 번에 한 번. 해제는 FinishChoosing에서 한 번.
            _isChoosing = true;
            _pauseService.RequestPause();
            ShowNextChoices();
        }

        private void ShowNextChoices()
        {
            // 1. 최대 레벨 전이고 지금 적용할 수 있는 강화만 후보로 담는다.
            _available.Clear();
            foreach (UpgradeData upgrade in _upgradePool)
            {
                if (GetLevel(upgrade) < upgrade.MaxLevel && upgrade.CanApply(_context))
                {
                    _available.Add(upgrade);
                }
            }

            // 2. 부분 Fisher–Yates: 앞에서 필요한 개수만큼만 섞는다. i번째 자리를 i~끝 중 아무 자리와 바꾼다.
            //    전체를 섞지 않아도 앞 i칸은 고르게 뽑힌 결과가 된다. Random.Range(int, int)는 끝 값을 포함하지 않는다.
            int count = Mathf.Min(ChoiceCount, _available.Count);
            for (int i = 0; i < count; i++)
            {
                int j = UnityEngine.Random.Range(i, _available.Count);
                UpgradeData temp = _available[i];
                _available[i] = _available[j];
                _available[j] = temp;
            }

            // 3. 앞에서 count개를 선택지로 만든다.
            _options.Clear();
            for (int i = 0; i < count; i++)
            {
                UpgradeData upgrade = _available[i];
                _options.Add(new UpgradeOption(upgrade, GetLevel(upgrade) + 1));
            }

            // 4. 모든 강화가 최대 레벨이면 고를 게 없으니 남은 레벨업을 버리고 재개한다.
            if (_options.Count == 0)
            {
                _pendingCount = 0;
                FinishChoosing();
                return;
            }

            ChoicesOffered?.Invoke(_options);
        }

        private void FinishChoosing()
        {
            _isChoosing = false;
            _pauseService.ReleasePause();
        }

        private int GetLevel(UpgradeData upgrade)
        {
            return _levels.TryGetValue(upgrade, out int level) ? level : 0;
        }
    }
}
