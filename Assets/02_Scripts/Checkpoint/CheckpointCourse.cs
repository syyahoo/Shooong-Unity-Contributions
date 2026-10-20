using UnityEngine;

public class CheckpointCourse : MonoBehaviour
{
    [Header("Checkpoints")] [SerializeField]
    private Checkpoint[] checkpoints;

    [Header("Settings")] [SerializeField] private bool initializeOnStart = true;

    [SerializeField] private FlightClearController flightClearController;
    [SerializeField] private CheckpointTelemetryProvider checkpointTelemetryProvider;
    
    private int _currentCheckpointIndex;
    private bool _isCompleted;

    public int CurrentCheckpointIndex => _currentCheckpointIndex;
    public int TotalCheckpointCount => checkpoints != null ? checkpoints.Length : 0;
    public bool IsCompleted => _isCompleted;

    // 시작 시 체크포인트 코스 초기화
    private void Start()
    {
        if (!initializeOnStart)
        {
            return;
        }

        InitializeCourse();
    }

    // 체크포인트 코스 초기화
    [ContextMenu("Initialize Course")]
    public void InitializeCourse()
    {
        if (checkpoints == null || checkpoints.Length == 0)
        {
            Debug.LogWarning("[CheckpointCourse] Checkpoints are missing.");
            return;
        }

        _currentCheckpointIndex = 0;
        _isCompleted = false;
        UpdateTelemetryProvider();

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == null)
            {
                continue;
            }

            checkpoints[i].Initialize(this, i);
            checkpoints[i].SetActiveState(i == _currentCheckpointIndex);
        }

        Debug.Log("[CheckpointCourse] Course initialized.");
    }

    // 현재 체크포인트 통과 처리
    public void PassCheckpoint(Checkpoint checkpoint)
    {
        if (_isCompleted)
        {
            return;
        }

        if (checkpoint == null)
        {
            return;
        }

        if (checkpoint.CheckpointIndex != _currentCheckpointIndex)
        {
            return;
        }

        Debug.Log($"[CheckpointCourse] Passed checkpoint {_currentCheckpointIndex + 1}/{TotalCheckpointCount}");

        checkpoint.SetActiveState(false);

        _currentCheckpointIndex++;

        if (_currentCheckpointIndex >= checkpoints.Length)
        {
            CompleteCourse();
            return;
        }

        checkpoints[_currentCheckpointIndex].SetActiveState(true);
        UpdateTelemetryProvider();
    }


    // 모든 체크포인트 완료 처리
    private void CompleteCourse()
    {
        _isCompleted = true;

        if (checkpointTelemetryProvider != null)
        {
            checkpointTelemetryProvider.MarkFinalArrived();
        }

        Debug.Log("[CheckpointCourse] Course Completed.");

        if (flightClearController != null)
        {
            flightClearController.ClearFlight();
        }
    }

    private void UpdateTelemetryProvider()
    {
        if (checkpointTelemetryProvider == null)
        {
            return;
        }

        checkpointTelemetryProvider.SetCurrentCheckpoint(_currentCheckpointIndex);
    }
}
