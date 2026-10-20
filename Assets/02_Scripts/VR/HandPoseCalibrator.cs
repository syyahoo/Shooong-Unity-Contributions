using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

using XRInputDevice = UnityEngine.XR.InputDevice;
using XRCommonUsages = UnityEngine.XR.CommonUsages;

public class HandPoseCalibrator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VRHandTracker handTracker;

    [Header("Auto Calibration")]
    [SerializeField] private bool calibrateOnStart = true;
    [SerializeField] private float autoCalibrationDelay = 1f;

    [Header("VR Calibration Button")]
    [SerializeField] private bool enableVRButtonCalibration = true;
    [SerializeField] private XRNode calibrationButtonHand = XRNode.RightHand;
    [SerializeField] private float calibrationHoldTime = 0.5f;

    [Header("Keyboard Fallback")]
    [SerializeField] private bool enableKeyboardFallback = true;

    private Quaternion _baseRotation;
    private Vector3 _basePosition;

    private float _calibrationHoldTimer;
    private bool _calibratedThisHold;

    public bool IsCalibrated { get; private set; }
    public Vector3 BasePosition => _basePosition;
    public Quaternion BaseRotation => _baseRotation;
    public Vector3 CurrentDeltaEuler { get; private set; }

    // 시작 시 자동 캘리브레이션 대기
    private IEnumerator Start()
    {
        if (!calibrateOnStart)
        {
            yield break;
        }

        yield return new WaitForSeconds(autoCalibrationDelay);

        Calibrate();
    }

    // 매 프레임 입력 처리와 기준 자세 대비 회전 차이 갱신
    private void Update()
    {
        HandleCalibrationInput();

        if (!IsCalibrated || handTracker == null || !handTracker.HasTarget)
        {
            return;
        }

        UpdateDeltaRotation();
    }

    // 키보드와 VR 버튼 캘리브레이션 입력 처리
    private void HandleCalibrationInput()
    {
        HandleKeyboardCalibration();
        HandleVRButtonCalibration();
    }

    // C 키 입력으로 기준 자세 재설정
    private void HandleKeyboardCalibration()
    {
        if (!enableKeyboardFallback)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            Calibrate();
        }
    }

    // 오른손 B 버튼을 길게 눌러 기준 자세 재설정
    private void HandleVRButtonCalibration()
    {
        if (!enableVRButtonCalibration)
        {
            return;
        }

        bool isPressed = IsCalibrationButtonPressed();

        if (!isPressed)
        {
            _calibrationHoldTimer = 0f;
            _calibratedThisHold = false;
            return;
        }

        _calibrationHoldTimer += Time.deltaTime;

        if (_calibrationHoldTimer < calibrationHoldTime)
        {
            return;
        }

        if (_calibratedThisHold)
        {
            return;
        }

        Calibrate();
        _calibratedThisHold = true;
    }

    // 오른손 Secondary Button 입력 상태 확인
    private bool IsCalibrationButtonPressed()
    {
        XRInputDevice device = InputDevices.GetDeviceAtXRNode(calibrationButtonHand);

        if (!device.isValid)
        {
            return false;
        }

        if (!device.TryGetFeatureValue(XRCommonUsages.secondaryButton, out bool secondaryButton))
        {
            return false;
        }

        return secondaryButton;
    }

    // 현재 트래커 자세를 기준 자세로 저장
    [ContextMenu("Calibrate Current Pose")]
    public void Calibrate()
    {
        if (handTracker == null || !handTracker.HasTarget)
        {
            Debug.LogWarning("[HandPoseCalibrator] HandTracker target is missing.");
            return;
        }

        HandPoseData pose = handTracker.CurrentPose;

        _basePosition = pose.Position;
        _baseRotation = pose.Rotation;
        CurrentDeltaEuler = Vector3.zero;
        IsCalibrated = true;

        Debug.Log("[HandPoseCalibrator] Calibration completed.");
    }

    // 기준 회전과 현재 회전의 차이 계산
    private void UpdateDeltaRotation()
    {
        Quaternion currentRotation = handTracker.CurrentPose.Rotation;
        Quaternion deltaRotation = Quaternion.Inverse(_baseRotation) * currentRotation;

        Vector3 rawEuler = deltaRotation.eulerAngles;

        CurrentDeltaEuler = new Vector3(
            NormalizeAngle(rawEuler.x),
            NormalizeAngle(rawEuler.y),
            NormalizeAngle(rawEuler.z)
        );
    }

    // 0~360도 값을 -180~180도 값으로 변환
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }
}