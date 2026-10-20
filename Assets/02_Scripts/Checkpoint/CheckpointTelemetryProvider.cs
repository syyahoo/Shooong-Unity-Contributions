using TMPro;
using UnityEngine;

public class CheckpointTelemetryProvider : MonoBehaviour
{
    private const string TargetStatus = "TARGET";
    private const string ArrivedStatus = "ARRIVED";

    [Header("Checkpoints")]
    [SerializeField] private Transform[] checkpoints;
    [SerializeField] private int currentCheckpointIndex;

    [Header("UI")]
    [SerializeField] private TMP_Text checkpointInfoText;

    private bool _isArrived;

    public int CurrentCheckpointIndex => currentCheckpointIndex;
    public int TotalCheckpointCount => checkpoints == null ? 0 : checkpoints.Length;
    public bool IsArrived => _isArrived;

    // 현재 목표 체크포인트 설정
    public void SetCurrentCheckpoint(int index)
    {
        if (checkpoints == null || checkpoints.Length == 0)
        {
            currentCheckpointIndex = 0;
            _isArrived = false;
            UpdateDisplay(null);
            return;
        }

        currentCheckpointIndex = Mathf.Clamp(index, 0, checkpoints.Length - 1);
        _isArrived = false;

        UpdateDisplay(null);
    }

    // 다음 체크포인트로 목표 갱신
    public void MoveToNextCheckpoint()
    {
        SetCurrentCheckpoint(currentCheckpointIndex + 1);
    }

    // 마지막 체크포인트 도착 처리
    public void MarkFinalArrived()
    {
        if (checkpoints == null || checkpoints.Length == 0)
        {
            _isArrived = true;
            UpdateDisplay(null);
            return;
        }

        currentCheckpointIndex = checkpoints.Length - 1;
        _isArrived = true;

        UpdateDisplay(null);
    }

    // 현재 체크포인트 payload 생성
    public DroneCheckpointTelemetryPayload CreatePayload(Vector3 dronePosition)
    {
        Transform target = GetCurrentCheckpoint();

        if (target == null)
        {
            return new DroneCheckpointTelemetryPayload(
                currentCheckpointIndex + 1,
                TotalCheckpointCount,
                "None",
                _isArrived ? ArrivedStatus : TargetStatus,
                true,
                0f,
                Vector3.zero);
        }

        float distance = Vector3.Distance(dronePosition, target.position);
        string status = _isArrived ? ArrivedStatus : TargetStatus;
        bool isFinalCheckpoint = currentCheckpointIndex >= TotalCheckpointCount - 1;

        UpdateDisplay(distance);

        return new DroneCheckpointTelemetryPayload(
            currentCheckpointIndex + 1,
            TotalCheckpointCount,
            target.name,
            status,
            isFinalCheckpoint,
            distance,
            target.position);
    }

    // 현재 목표 체크포인트 반환
    private Transform GetCurrentCheckpoint()
    {
        if (checkpoints == null || checkpoints.Length == 0)
        {
            return null;
        }

        if (currentCheckpointIndex < 0 || currentCheckpointIndex >= checkpoints.Length)
        {
            return null;
        }

        return checkpoints[currentCheckpointIndex];
    }

    // 체크포인트 UI 표시 갱신
    private void UpdateDisplay(float? distance)
    {
        if (checkpointInfoText == null)
        {
            return;
        }

        if (_isArrived)
        {
            checkpointInfoText.text = $"CP {TotalCheckpointCount}/{TotalCheckpointCount}  도착";
            return;
        }

        if (TotalCheckpointCount <= 0)
        {
            checkpointInfoText.text = "CP 없음";
            return;
        }

        string distanceText = distance.HasValue ? $"{distance.Value:0.0} m" : "-";
        checkpointInfoText.text = $"CP {currentCheckpointIndex + 1}/{TotalCheckpointCount}  {distanceText}";
    }
}