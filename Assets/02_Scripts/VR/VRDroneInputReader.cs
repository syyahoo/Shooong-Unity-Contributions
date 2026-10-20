using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

using XRInputDevice = UnityEngine.XR.InputDevice;
using XRCommonUsages = UnityEngine.XR.CommonUsages;

public class VRDroneInputReader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HandPoseCalibrator calibrator;

    [Header("Tilt Input Settings")]
    [SerializeField] private float maxTiltAngle = 30f;
    [SerializeField] private float tiltDeadZone = 3f;
    [SerializeField] private bool invertPitchInput = true;
    [SerializeField] private bool invertRollInput = false;

    [Header("Joystick Settings")]
    [SerializeField] private XRNode joystickHand = XRNode.RightHand;
    [SerializeField] private float joystickDeadZone = 0.15f;
    [SerializeField] private bool useDominantAxisFilter = true;

    [Header("Hover Input")]
    [SerializeField] private XRNode hoverButtonHand = XRNode.RightHand;
    [SerializeField] private bool enableKeyboardHoverFallback = true;

    [Header("Keyboard Fallback")]
    [SerializeField] private bool enableKeyboardFallback = true;

    public DroneInputData CurrentInput { get; private set; }

    // 매 프레임 트래커, 조이스틱, 호버 입력을 드론 입력값으로 변환
    private void Update()
    {
        if (calibrator == null || !calibrator.IsCalibrated)
        {
            CurrentInput = new DroneInputData(Vector2.zero, 0f, 0f, false, false);
            return;
        }

        Vector2 moveInput = ReadTiltMoveInput();
        Vector2 joystickInput = ReadJoystickInput();

        if (enableKeyboardFallback)
        {
            joystickInput += ReadKeyboardInput();
            joystickInput = Vector2.ClampMagnitude(joystickInput, 1f);
        }

        joystickInput = ApplyJoystickFilter(joystickInput);

        bool hoverInput = ReadHoverInput();

        CurrentInput = new DroneInputData(
            moveInput,
            joystickInput.x,
            joystickInput.y,
            true,
            hoverInput
        );
    }

    // 트래커 기울기 값을 전후좌우 이동 입력으로 변환
    private Vector2 ReadTiltMoveInput()
    {
        Vector3 delta = calibrator.CurrentDeltaEuler;

        float forwardInput = ConvertAngleToInput(delta.x, invertPitchInput);
        float sideInput = ConvertAngleToInput(delta.z, invertRollInput);

        return new Vector2(sideInput, forwardInput);
    }

    // 오른손 조이스틱 입력 읽기
    private Vector2 ReadJoystickInput()
    {
        XRInputDevice device = InputDevices.GetDeviceAtXRNode(joystickHand);

        if (!device.isValid)
        {
            return Vector2.zero;
        }

        if (!device.TryGetFeatureValue(XRCommonUsages.primary2DAxis, out Vector2 axis))
        {
            return Vector2.zero;
        }

        if (axis.magnitude < joystickDeadZone)
        {
            return Vector2.zero;
        }

        return axis;
    }

    // 키보드 테스트용 조이스틱 입력 읽기
    private Vector2 ReadKeyboardInput()
    {
        if (Keyboard.current == null)
        {
            return Vector2.zero;
        }

        float x = 0f;
        float y = 0f;

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            x -= 1f;
        }

        if (Keyboard.current.rightArrowKey.isPressed)
        {
            x += 1f;
        }

        if (Keyboard.current.upArrowKey.isPressed)
        {
            y += 1f;
        }

        if (Keyboard.current.downArrowKey.isPressed)
        {
            y -= 1f;
        }

        return new Vector2(x, y);
    }

    // 오른손 A 버튼 또는 Space 키로 호버 입력 읽기
    private bool ReadHoverInput()
    {
        bool isPressed = false;

        XRInputDevice device = InputDevices.GetDeviceAtXRNode(hoverButtonHand);

        if (device.isValid &&
            device.TryGetFeatureValue(XRCommonUsages.primaryButton, out bool primaryButton))
        {
            isPressed |= primaryButton;
        }

        if (enableKeyboardHoverFallback &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.isPressed)
        {
            isPressed = true;
        }

        return isPressed;
    }

    // 조이스틱 입력 필터 적용
    private Vector2 ApplyJoystickFilter(Vector2 input)
    {
        if (input == Vector2.zero)
        {
            return Vector2.zero;
        }

        if (!useDominantAxisFilter)
        {
            return input;
        }

        return ApplyDominantAxisFilter(input);
    }

    // 더 큰 축만 남기고 작은 축 입력 제거
    private Vector2 ApplyDominantAxisFilter(Vector2 input)
    {
        float absX = Mathf.Abs(input.x);
        float absY = Mathf.Abs(input.y);

        if (absX > absY)
        {
            return new Vector2(input.x, 0f);
        }

        if (absY > absX)
        {
            return new Vector2(0f, input.y);
        }

        return Vector2.zero;
    }

    // 각도 값을 -1~1 입력값으로 변환
    private float ConvertAngleToInput(float angle, bool invert)
    {
        if (Mathf.Abs(angle) < tiltDeadZone)
        {
            return 0f;
        }

        float input = Mathf.Clamp(angle / maxTiltAngle, -1f, 1f);

        if (invert)
        {
            input *= -1f;
        }

        return input;
    }
}