using UnityEngine;

public class VRHandTracker : MonoBehaviour
{
    [Header("Tracker Target")]
    [SerializeField] private Transform targetTracker;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private float logInterval = 0.5f;

    private float _logTimer;

    public HandPoseData CurrentPose { get; private set; }
    public bool HasTarget => targetTracker != null;

    // 매 프레임 트래커 위치/회전 갱신
    private void Update()
    {
        if (!HasTarget)
        {
            return;
        }

        UpdatePose();
        PrintDebugLog();
    }

    // 현재 트래커 Transform 정보를 저장
    private void UpdatePose()
    {
        CurrentPose = new HandPoseData(targetTracker);
    }

    // 일정 간격으로 트래커 좌표 출력
    private void PrintDebugLog()
    {
        if (!enableDebugLog)
        {
            return;
        }

        _logTimer += Time.deltaTime;

        if (_logTimer < logInterval)
        {
            return;
        }

        _logTimer = 0f;

        Debug.Log(
            $"[VRHandTracker] Position: {CurrentPose.Position}, Rotation: {CurrentPose.EulerAngles}"
        );
    }

    // 씬 뷰에서 트래커 방향 시각화
    private void OnDrawGizmos()
    {
        if (targetTracker == null)
        {
            return;
        }

        Gizmos.DrawRay(targetTracker.position, targetTracker.forward * 0.3f);
    }
}