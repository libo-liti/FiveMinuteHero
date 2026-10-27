using UnityEngine;
using UnityEngine.InputSystem;

namespace FiveMinuteHero.Player
{
    // Move 액션(WASD + 가상 조이스틱)의 값을 읽는다. PlayerMovement가 이것을 참조한다.
    public class ActionMoveInput : MonoBehaviour, IMoveInput
    {
        [SerializeField] private InputActionReference _moveAction;

        public Vector2 Direction => _moveAction.action.ReadValue<Vector2>();

        private void OnEnable()
        {
            _moveAction.action.Enable();
        }

        private void OnDisable()
        {
            _moveAction.action.Disable();
        }
    }
}
