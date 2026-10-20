using UnityEngine;

public class FlightClearController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DroneHoverLock droneHoverLock;
    [SerializeField] private FlightResultUI resultUI;
    [SerializeField] private CheckpointTelemetryProvider checkpointTelemetryProvider;
    [SerializeField] private DronePoseBatchSender poseBatchSender;

    private float _startTime;
    private bool _isCleared;

    // 비행 시작 시간 기록
    private void Start()
    {
        _startTime = Time.time;
    }

    // 위치 전송기 설정
    public void SetPoseBatchSender(DronePoseBatchSender sender)
    {
        poseBatchSender = sender;
    }

    // 비행 클리어 처리
    public void ClearFlight()
    {
        if (_isCleared)
        {
            return;
        }

        _isCleared = true;

        float clearTime = Time.time - _startTime;

        if (checkpointTelemetryProvider != null)
        {
            checkpointTelemetryProvider.MarkFinalArrived();
        }

        if (poseBatchSender != null)
        {
            poseBatchSender.StopAndFlushNow();
        }

        if (droneHoverLock != null)
        {
            droneHoverLock.LockHover();
        }

        if (resultUI != null)
        {
            resultUI.Show(clearTime);
        }

        Debug.Log($"[FlightClearController] Clear Time: {clearTime:F3}");
    }
}
