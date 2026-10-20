using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronePoseBatchSender : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ApiConfigSO apiConfig;
    [SerializeField] private ApiClient apiClient;
    [SerializeField] private Transform targetDrone;

    [Header("Checkpoint")]
    [SerializeField] private CheckpointTelemetryProvider checkpointTelemetryProvider;

    [Header("Runtime")]
    [SerializeField] private bool sendOnStart;
    [SerializeField] private float sendInterval = 0.5f;
    [SerializeField] private int flightId;
    [SerializeField] private int maxBufferSize = 300;

    [Header("Endpoint")]
    [SerializeField] private bool useApiConfigEndpoint = true;
    [SerializeField] private string customEndpointPath = "/api/flights/{flightId}/poses";

    [Header("Debug")]
    [SerializeField] private bool logPayload;
    [SerializeField] private bool logSampleCount = true;

    private readonly List<DronePoseTelemetrySample> _sampleBuffer = new List<DronePoseTelemetrySample>();
    private readonly List<DronePoseTelemetrySample> _sendingBuffer = new List<DronePoseTelemetrySample>();

    private Coroutine _sendCoroutine;
    private int _sequence;
    private float _startRealtimeSeconds;
    private bool _isSampling;

    public int FlightId => flightId;
    public bool IsSampling => _isSampling;

    // 시작 시 자동 전송 옵션 처리
    private void Start()
    {
        if (sendOnStart)
        {
            StartSending();
        }
    }

    // 매 프레임 드론 위치 샘플 저장
    private void LateUpdate()
    {
        if (!_isSampling)
        {
            return;
        }

        SampleCurrentPose();
    }

    // 비행 ID 설정
    public void SetFlightId(int newFlightId)
    {
        flightId = newFlightId;
        Debug.Log($"[DronePoseBatchSender] Flight Id set: {flightId}");
    }

    // 기존 OrderId 호출 호환용
    public void SetOrderId(int newOrderId)
    {
        SetFlightId(newOrderId);
    }

    // 드론 Transform 설정
    public void SetTargetDrone(Transform newTargetDrone)
    {
        targetDrone = newTargetDrone;

        if (targetDrone == null)
        {
            Debug.LogWarning("[DronePoseBatchSender] Target Drone cleared.");
            return;
        }

        Debug.Log($"[DronePoseBatchSender] Target Drone set: {targetDrone.name}");
    }

    // 체크포인트 정보 제공자 설정
    public void SetCheckpointTelemetryProvider(CheckpointTelemetryProvider provider)
    {
        checkpointTelemetryProvider = provider;
    }

    // 위치 샘플링과 주기 전송 시작
    [ContextMenu("Start Sending")]
    public void StartSending()
    {
        if (_isSampling)
        {
            return;
        }

        if (!CanStart())
        {
            return;
        }

        _sampleBuffer.Clear();
        _sendingBuffer.Clear();

        _sequence = 0;
        _startRealtimeSeconds = Time.realtimeSinceStartup;
        _isSampling = true;

        if (_sendCoroutine != null)
        {
            StopCoroutine(_sendCoroutine);
        }

        _sendCoroutine = StartCoroutine(SendLoop());

        Debug.Log("[DronePoseBatchSender] Sampling and sending started.");
    }

    // 위치 샘플링과 주기 전송 정지
    [ContextMenu("Stop Sending")]
    public void StopSending()
    {
        _isSampling = false;

        if (_sendCoroutine != null)
        {
            StopCoroutine(_sendCoroutine);
            _sendCoroutine = null;
        }

        Debug.Log("[DronePoseBatchSender] Sampling and sending stopped.");
    }

    // 샘플링을 정지하고 남은 위치 데이터를 즉시 전송
    [ContextMenu("Stop And Flush Now")]
    public void StopAndFlushNow()
    {
        if (!_isSampling)
        {
            StartCoroutine(SendBufferedSamples());
            return;
        }

        _isSampling = false;

        if (targetDrone != null)
        {
            SampleCurrentPose();
        }

        if (_sendCoroutine != null)
        {
            StopCoroutine(_sendCoroutine);
            _sendCoroutine = null;
        }

        StartCoroutine(SendBufferedSamples());

        Debug.Log("[DronePoseBatchSender] Sampling stopped and final payload flush requested.");
    }

    // 버퍼 데이터를 즉시 1회 전송
    [ContextMenu("Send Buffered Samples Once")]
    public void SendBufferedSamplesOnce()
    {
        StartCoroutine(SendBufferedSamples());
    }

    // 시작 가능 여부 확인
    private bool CanStart()
    {
        if (apiConfig == null)
        {
            Debug.LogWarning("[DronePoseBatchSender] ApiConfig is missing.");
            return false;
        }

        if (apiClient == null)
        {
            Debug.LogWarning("[DronePoseBatchSender] ApiClient is missing.");
            return false;
        }

        if (targetDrone == null)
        {
            Debug.LogWarning("[DronePoseBatchSender] Target Drone is missing.");
            return false;
        }

        if (flightId <= 0)
        {
            Debug.LogWarning("[DronePoseBatchSender] Flight Id is invalid.");
            return false;
        }

        return true;
    }

    // 현재 드론 위치 샘플 저장
    private void SampleCurrentPose()
    {
        if (targetDrone == null)
        {
            return;
        }

        if (_sampleBuffer.Count >= maxBufferSize)
        {
            _sampleBuffer.RemoveAt(0);
        }

        float elapsedTime = Time.realtimeSinceStartup - _startRealtimeSeconds;

        DroneCheckpointTelemetryPayload checkpointPayload = null;

        if (checkpointTelemetryProvider != null)
        {
            checkpointPayload = checkpointTelemetryProvider.CreatePayload(targetDrone.position);
        }

        DronePoseTelemetrySample sample = new DronePoseTelemetrySample(
            _sequence++,
            elapsedTime,
            targetDrone.position,
            targetDrone.rotation,
            checkpointPayload);

        _sampleBuffer.Add(sample);
    }

    // 일정 간격으로 버퍼 데이터를 전송
    private IEnumerator SendLoop()
    {
        while (_isSampling)
        {
            yield return new WaitForSeconds(sendInterval);
            yield return SendBufferedSamples();
        }
    }

    // 버퍼에 저장된 위치 데이터를 서버로 전송
    private IEnumerator SendBufferedSamples()
    {
        if (_sampleBuffer.Count <= 0)
        {
            yield break;
        }

        if (apiConfig == null || apiClient == null)
        {
            Debug.LogWarning("[DronePoseBatchSender] ApiConfig or ApiClient is missing.");
            yield break;
        }

        if (flightId <= 0)
        {
            Debug.LogWarning("[DronePoseBatchSender] Flight Id is invalid.");
            yield break;
        }

        _sendingBuffer.Clear();
        _sendingBuffer.AddRange(_sampleBuffer);
        _sampleBuffer.Clear();

        DronePoseBatchRequest payload = new DronePoseBatchRequest(flightId, _sendingBuffer);
        string json = payload.ToJson();

        if (logSampleCount)
        {
            Debug.Log($"[DronePoseBatchSender] Sending sample count: {_sendingBuffer.Count}");
        }

        if (logPayload)
        {
            Debug.Log($"[DronePoseBatchSender] Payload: {json}");
        }

        string url = BuildSendUrl();
        bool isSuccess = false;

        yield return apiClient.PostJson(
            url,
            json,
            true,
            response =>
            {
                isSuccess = true;
                Debug.Log("[DronePoseBatchSender] Pose batch sent.");
            },
            (statusCode, error) =>
            {
                isSuccess = false;
                Debug.LogWarning($"[DronePoseBatchSender] Pose batch failed. Status: {statusCode}, Error: {error}");
            });

        if (isSuccess)
        {
            _sendingBuffer.Clear();
            yield break;
        }

        RestoreSendingBuffer();
    }

    // 전송 URL 생성
    private string BuildSendUrl()
    {
        if (useApiConfigEndpoint)
        {
            return apiConfig.BuildDronePoseBatchUrl(flightId);
        }

        string path = customEndpointPath
            .Replace("{flightId}", flightId.ToString())
            .Replace("{orderId}", flightId.ToString());

        return apiConfig.BuildUrl(path);
    }

    // 전송 실패 시 전송 중이던 샘플을 버퍼로 복구
    private void RestoreSendingBuffer()
    {
        if (_sendingBuffer.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < _sendingBuffer.Count; i++)
        {
            if (_sampleBuffer.Count >= maxBufferSize)
            {
                _sampleBuffer.RemoveAt(0);
            }

            _sampleBuffer.Add(_sendingBuffer[i]);
        }

        _sendingBuffer.Clear();

        Debug.LogWarning("[DronePoseBatchSender] Pose batch failed. Samples restored to buffer.");
    }
}