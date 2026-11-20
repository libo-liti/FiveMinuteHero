using FiveMinuteHero.Flow;
using UnityEngine;

namespace FiveMinuteHero.Items
{
    // 플레이어에 붙어 아이템을 받는 창구. 바닥 아이템(ItemPickup)은 효과를 직접 내지 않고 여기에 넘긴다.
    // "판이 끝나면 효과 없음" 검사를 이 한 곳에 모은다. 아이템 종류가 늘어도, 아이템이 상자에서 나와도 같은 검사를 거친다.
    // 플레이어 위치(transform)도 이 컴포넌트로 읽는다. 바닥 아이템이 줍는 거리를 잴 때 쓴다.
    public class PlayerItemCollector : MonoBehaviour
    {
        private ItemContext _context;
        private IRunState _runState;

        // Bootstrap이 호출한다. 판 상태는 IRunState로만 받아 판을 끝내거나 멈추는 기능은 부를 수 없다.
        public void Init(ItemContext context, IRunState runState)
        {
            _context = context;
            _runState = runState;
        }

        private void Start()
        {
            if (_context == null || _runState == null)
            {
                Debug.LogError("PlayerItemCollector: Init을 받지 못했습니다. 씬의 Bootstrap 연결을 확인하세요.", this);
            }
        }

        // 효과를 냈으면 true. 바닥 아이템은 true일 때만 사라진다(효과 없이 아이템만 없어지는 일을 막는다).
        public bool TryUse(ItemData item)
        {
            // timeScale 0은 이동만 멈추고 Update는 계속 돈다. 판이 끝난 뒤 멈춘 화면에서도 거리 검사에 걸려 먹힐 수 있어서 여기서 막는다.
            // 사망도 판 종료(EndRun)를 거치므로 이 검사 하나로 사망·클리어를 함께 막는다.
            if (_runState.IsRunOver)
            {
                return false;
            }

            item.Apply(_context);
            return true;
        }
    }
}
