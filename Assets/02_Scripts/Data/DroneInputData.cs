using UnityEngine;

public struct DroneInputData
{
    public Vector2 MoveInput;
    public float YawInput;
    public float ThrottleInput;
    public bool IsControlling;
    public bool HoverInput;

    // 드론 조종 입력 데이터 생성
    public DroneInputData(
        Vector2 moveInput,
        float yawInput,
        float throttleInput,
        bool isControlling,
        bool hoverInput = false)
    {
        MoveInput = moveInput;
        YawInput = yawInput;
        ThrottleInput = throttleInput;
        IsControlling = isControlling;
        HoverInput = hoverInput;
    }
}
