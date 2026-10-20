using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject visualObject;

    [Header("Collider")]
    [SerializeField] private Collider triggerCollider;

    private CheckpointCourse _course;
    private int _checkpointIndex;
    private bool _isActive;

    public int CheckpointIndex => _checkpointIndex;
    public bool IsActive => _isActive;

    // 체크포인트 기본 참조 자동 연결
    private void Reset()
    {
        triggerCollider = GetComponent<Collider>();

        if (transform.childCount > 0)
        {
            visualObject = transform.GetChild(0).gameObject;
        }
    }

    // 체크포인트 초기화
    public void Initialize(CheckpointCourse course, int checkpointIndex)
    {
        _course = course;
        _checkpointIndex = checkpointIndex;
    }

    // 체크포인트 활성/비활성 처리
    public void SetActiveState(bool active)
    {
        _isActive = active;

        if (visualObject != null)
        {
            visualObject.SetActive(active);
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = active;
        }
    }

    // 드론이 체크포인트에 진입했는지 확인
    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive)
        {
            return;
        }

        if (!IsPlayerDrone(other))
        {
            return;
        }

        _course?.PassCheckpoint(this);
    }

    // 충돌 대상이 플레이어 드론인지 확인
    private bool IsPlayerDrone(Collider other)
    {
        if (other.attachedRigidbody == null)
        {
            return false;
        }

        return other.attachedRigidbody.GetComponent<DroneMovement>() != null;
    }
}
