using TMPro;
using UnityEngine;

public class VRPoseDebugView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VRHandTracker handTracker;
    [SerializeField] private HandPoseCalibrator calibrator;
    [SerializeField] private VRDroneInputReader inputReader;
    [SerializeField] private TMP_Text poseText;

    [Header("Settings")]
    [SerializeField] private int decimalPlaces = 2;

    // 매 프레임 VR 디버그 UI 갱신
    private void Update()
    {
        if (handTracker == null || poseText == null)
        {
            return;
        }

        if (!handTracker.HasTarget)
        {
            poseText.text = "Tracker: None";
            return;
        }

        UpdatePoseText();
    }

    // 트래커 값, 델타 값, 드론 입력값 표시
    private void UpdatePoseText()
    {
        HandPoseData pose = handTracker.CurrentPose;

        poseText.text =
            "[Tracker Debug]\n\n" +
            $"Position\n" +
            $"X: {Format(pose.Position.x)}\n" +
            $"Y: {Format(pose.Position.y)}\n" +
            $"Z: {Format(pose.Position.z)}\n\n" +
            $"Delta Rotation\n" +
            GetDeltaText() + "\n\n" +
            $"Drone Input\n" +
            GetDroneInputText();
    }

    // 기준 자세 대비 회전 변화량 텍스트 생성
    private string GetDeltaText()
    {
        if (calibrator == null)
        {
            return "Calibration: None";
        }

        if (!calibrator.IsCalibrated)
        {
            return "Calibration: Waiting";
        }

        Vector3 delta = calibrator.CurrentDeltaEuler;

        return
            $"Pitch X: {Format(delta.x)}\n" +
            $"Yaw Y: {Format(delta.y)}\n" +
            $"Roll Z: {Format(delta.z)}";
    }

    // 드론 조종 입력값 텍스트 생성
    private string GetDroneInputText()
    {
        if (inputReader == null)
        {
            return "Input Reader: None";
        }

        DroneInputData input = inputReader.CurrentInput;

        return
            $"Move X: {Format(input.MoveInput.x)}\n" +
            $"Move Y: {Format(input.MoveInput.y)}\n" +
            $"Yaw: {Format(input.YawInput)}\n" +
            $"Throttle: {Format(input.ThrottleInput)}\n" +
            $"Control: {input.IsControlling}";
    }

    // 소수점 자리수 정리
    private string Format(float value)
    {
        return value.ToString($"F{decimalPlaces}");
    }
}