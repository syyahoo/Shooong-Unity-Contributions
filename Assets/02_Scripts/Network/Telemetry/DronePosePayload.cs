using System;
using System.Collections.Generic;
using UnityEngine;

public static class DroneTelemetryNumberFormatter
{
    // 위치값 반올림
    public static float Position(float value)
    {
        return Mathf.Round(value * 100f) / 100f;
    }

    // 회전값 반올림
    public static float Rotation(float value)
    {
        return Mathf.Round(value * 10000f) / 10000f;
    }

    // 시간값 반올림
    public static float Time(float value)
    {
        return Mathf.Round(value * 1000f) / 1000f;
    }
}

[Serializable]
public class DroneTelemetryVector3Payload
{
    public float x;
    public float y;
    public float z;

    // Vector3 값을 전송용 좌표 데이터로 변환
    public DroneTelemetryVector3Payload(Vector3 value)
    {
        x = DroneTelemetryNumberFormatter.Position(value.x);
        y = DroneTelemetryNumberFormatter.Position(value.y);
        z = DroneTelemetryNumberFormatter.Position(value.z);
    }
}

[Serializable]
public class DroneTelemetryQuaternionPayload
{
    public float x;
    public float y;
    public float z;
    public float w;

    // Quaternion 값을 전송용 회전 데이터로 변환
    public DroneTelemetryQuaternionPayload(Quaternion value)
    {
        x = DroneTelemetryNumberFormatter.Rotation(value.x);
        y = DroneTelemetryNumberFormatter.Rotation(value.y);
        z = DroneTelemetryNumberFormatter.Rotation(value.z);
        w = DroneTelemetryNumberFormatter.Rotation(value.w);
    }
}

[Serializable]
public class DroneCheckpointTelemetryPayload
{
    public int targetCheckpointIndex;
    public int totalCheckpointCount;
    public string targetCheckpointName;
    public string targetCheckpointStatus;
    public bool isFinalCheckpoint;
    public float distanceToTarget;
    public DroneTelemetryVector3Payload targetCheckpointPosition;

    // 체크포인트 상태를 전송용 데이터로 생성
    public DroneCheckpointTelemetryPayload(
        int targetCheckpointIndex,
        int totalCheckpointCount,
        string targetCheckpointName,
        string targetCheckpointStatus,
        bool isFinalCheckpoint,
        float distanceToTarget,
        Vector3 targetCheckpointPosition)
    {
        this.targetCheckpointIndex = targetCheckpointIndex;
        this.totalCheckpointCount = totalCheckpointCount;
        this.targetCheckpointName = targetCheckpointName;
        this.targetCheckpointStatus = targetCheckpointStatus;
        this.isFinalCheckpoint = isFinalCheckpoint;
        this.distanceToTarget = DroneTelemetryNumberFormatter.Position(distanceToTarget);
        this.targetCheckpointPosition = new DroneTelemetryVector3Payload(targetCheckpointPosition);
    }
}

[Serializable]
public class DronePoseTelemetrySample
{
    public int sequence;
    public long timestampUnixMs;
    public float elapsedTimeSeconds;
    public DroneTelemetryVector3Payload position;
    public DroneTelemetryQuaternionPayload rotation;
    public DroneCheckpointTelemetryPayload checkpoint;

    // 드론 위치 샘플 생성
    public DronePoseTelemetrySample(
        int sequence,
        float elapsedTimeSeconds,
        Vector3 position,
        Quaternion rotation,
        DroneCheckpointTelemetryPayload checkpoint)
    {
        this.sequence = sequence;
        timestampUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        this.elapsedTimeSeconds = DroneTelemetryNumberFormatter.Time(elapsedTimeSeconds);
        this.position = new DroneTelemetryVector3Payload(position);
        this.rotation = new DroneTelemetryQuaternionPayload(rotation);
        this.checkpoint = checkpoint;
    }
}

[Serializable]
public class DronePoseBatchRequest
{
    public int flightId;
    public List<DronePoseTelemetrySample> samples;

    // 위치 샘플 묶음 요청 데이터 생성
    public DronePoseBatchRequest(int flightId, List<DronePoseTelemetrySample> samples)
    {
        this.flightId = flightId;
        this.samples = samples;
    }

    // JSON 문자열 변환
    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }
}