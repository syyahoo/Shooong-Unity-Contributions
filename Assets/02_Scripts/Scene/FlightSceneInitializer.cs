using System.Collections;
using UnityEngine;

public class FlightSceneInitializer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerDrone;

    [Header("Telemetry")]
    [SerializeField] private bool startTelemetryOnStart = true;
    [SerializeField] private float startDelay = 0.5f;
    
    [SerializeField] private CheckpointTelemetryProvider checkpointTelemetryProvider;
    [SerializeField] private FlightClearController flightClearController;

    private DronePoseBatchSender _poseBatchSender;

    // Flight 씬 시작 후 드론 위치 전송 초기화
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(startDelay);

        InitializeFlightScene();
    }

    // Flight 씬 네트워크 참조 연결
    [ContextMenu("Initialize Flight Scene")]
    public void InitializeFlightScene()
    {
        if (!TryGetPoseBatchSender())
        {
            return;
        }

        if (playerDrone == null)
        {
            Debug.LogWarning("[FlightSceneInitializer] PlayerDrone is missing.");
            return;
        }

        _poseBatchSender.SetTargetDrone(playerDrone);
        _poseBatchSender.SetCheckpointTelemetryProvider(checkpointTelemetryProvider);

        if (flightClearController != null)
        {
            flightClearController.SetPoseBatchSender(_poseBatchSender);
        }

        if (!startTelemetryOnStart)
        {
            return;
        }

        StartTelemetry();
    }

    // 드론 위치 전송 시작
    public void StartTelemetry()
    {
        if (_poseBatchSender == null)
        {
            Debug.LogWarning("[FlightSceneInitializer] PoseBatchSender is missing.");
            return;
        }

        if (_poseBatchSender.IsSampling)
        {
            return;
        }

        _poseBatchSender.StartSending();
    }

    // 드론 위치 전송 정지
    public void StopTelemetry()
    {
        if (_poseBatchSender == null)
        {
            return;
        }

        _poseBatchSender.StopSending();
    }

    // BackendManager에서 DronePoseBatchSender 찾기
    private bool TryGetPoseBatchSender()
    {
        if (BackendManager.Instance == null)
        {
            Debug.LogWarning("[FlightSceneInitializer] BackendManager instance is missing.");
            return false;
        }

        _poseBatchSender = BackendManager.Instance.GetComponent<DronePoseBatchSender>();

        if (_poseBatchSender == null)
        {
            Debug.LogWarning("[FlightSceneInitializer] DronePoseBatchSender is missing.");
            return false;
        }

        return true;
    }
    
    // 체크포인트 정보 제공자 설정
    public void SetCheckpointTelemetryProvider(CheckpointTelemetryProvider provider)
    {
        checkpointTelemetryProvider = provider;
    }
}
