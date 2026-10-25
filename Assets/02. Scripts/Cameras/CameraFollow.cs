using UnityEngine;

namespace FiveMinuteHero.Cameras
{
    // 카메라가 타깃의 x, y를 매 프레임 그대로 따라간다(즉시 추적).
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;

        // 타깃 이동(Update·물리 보간)이 끝난 뒤의 최종 위치를 따라가기 위해 LateUpdate에서 처리한다.
        private void LateUpdate()
        {
            Vector3 targetPosition = _target.position;
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        }
    }
}
