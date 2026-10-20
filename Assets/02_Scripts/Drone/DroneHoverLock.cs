using UnityEngine;

public class DroneHoverLock : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonoBehaviour droneMovement;
    [SerializeField] private Rigidbody droneRigidbody;

    // 컴포넌트 참조 자동 연결
    private void Reset()
    {
        droneMovement = GetComponent<MonoBehaviour>();
        droneRigidbody = GetComponent<Rigidbody>();
    }

    // 드론을 현재 위치에서 정지 상태로 고정
    public void LockHover()
    {
        if (droneMovement != null)
        {
            droneMovement.enabled = false;
        }

        if (droneRigidbody == null)
        {
            return;
        }

        droneRigidbody.linearVelocity = Vector3.zero;
        droneRigidbody.angularVelocity = Vector3.zero;
        droneRigidbody.useGravity = false;
        droneRigidbody.isKinematic = true;
    }
}