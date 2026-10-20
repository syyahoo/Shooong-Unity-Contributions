using UnityEngine;

public struct HandPoseData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 EulerAngles;
    public Vector3 Forward;
    public Vector3 Up;
    public Vector3 Right;

    // 손 위치/회전 데이터 생성
    public HandPoseData(Transform target)
    {
        Position = target.position;
        Rotation = target.rotation;
        EulerAngles = target.rotation.eulerAngles;
        Forward = target.forward;
        Up = target.up;
        Right = target.right;
    }
}